using System;
using System.Collections.Generic;

namespace TaoTie
{
    public class AbilityComponent : Component, IComponent<List<ConfigAbility>>
    {
        private bool isDestroy;
        private LinkedListComponent<ActorAbility> abilities;

        private LinkedListComponent<ActorModifier> modifiers;
        private UnOrderMultiMap<string, ActorModifier> modifierDictionary; //[Ability_Modifier:ActorModifier]

        /// <summary>批量摘除期间挂起通知：一次减 N 层只做一次全量 Rebuild。</summary>
        private bool suppressNotify;

        /// <summary>本帧是否已登记待摘除 Flush（同帧多次减层只调度一次）。</summary>
        private bool flushScheduled;

        /// <summary>
        /// 挂起的「恢复型」补执行（当前全库只有 <c>KeQing_NormalAttack4_Show</c> 这一类的 Show 端）。
        /// key = 能力名，value = 该次挂起的取消令牌。
        ///
        /// 连段内过渡要按美术节奏等到原 StartTime 才回显，所以必须延迟；但一旦进入收剑态
        /// （冲刺 / 受击 / 死亡 / 收剑），这层延迟就变成「角色或武器一直不可见」的尾巴，
        /// 必须由 <see cref="FlushPendingRestores"/> 立刻兑现。详见 <c>ExecuteAbilityClip.OnBreak</c>。
        /// </summary>
        private Dictionary<string, ETCancellationToken> pendingRestores;

        /// <summary>
        /// 活跃 modifier 链表（只读遍历）。供 <see cref="ControlComponent"/> 全量聚合控制位，
        /// 用链表直读而不是回调委托，避免每次重算都分配闭包。
        /// 不要在外部增删，统一走 ApplyModifier / RemoveModifier。
        /// </summary>
        public LinkedListComponent<ActorModifier> Modifiers => modifiers;
        
        #region override

        public void Init(List<ConfigAbility> data)
        {
            isDestroy = false;
            pendingRestores = null;
            abilities = LinkedListComponent<ActorAbility>.Create();
            modifiers = LinkedListComponent<ActorModifier>.Create();
            modifierDictionary = new UnOrderMultiMap<string, ActorModifier>();
            if (data != null)
            {
                for (int i = 0; i < data.Count; i++)
                {
                    var ability = ActorAbility.Create(Id, data[i], this);
                    abilities.AddLast(ability);
                    ability.AfterAdd();
                }
            }
        }

        public void Destroy()
        {
            if (isDestroy) return;
            isDestroy = true;

            DropPendingRestores();
            
            for (var node = abilities.Last; node != null;)
            {
                var prev = node.Previous;
                node.Value.BeforeRemove();
                node.Value.Dispose();
                node = prev;
            }

            abilities.Dispose();

            for (var node = modifiers.Last; node != null;)
            {
                var prev = node.Previous;
                node.Value.BeforeRemove();
                node.Value.Dispose();
                node = prev;
            }

            modifiers.Dispose();

            modifierDictionary = null;
            suppressNotify = false;
            flushScheduled = false;
        }

        #endregion
        public ActorAbility AddAbility(ConfigAbility config)
        {
            //组件销毁中禁止再添加
            if (isDestroy) return null;
            var ability = ActorAbility.Create(Id, config, this);
            abilities.AddLast(ability);
            ability.AfterAdd();
            return ability;
        }
        public void AddAbilities(List<ConfigAbility> configs)
        {
            for (int i = 0; i < configs.Count; i++)
            {
                AddAbility(configs[i]);
            }
        }

        public ActorModifier ApplyModifier(long applierID, ActorAbility ability, string modifierName)
        {
            //组件销毁中禁止再添加
            if (isDestroy) return null;
            if (!ability.TryGetConfigAbilityModifier(modifierName, out var config))
            {
                Log.Error(
                    $"ApplyModifier Fail! modifierName = {modifierName} abilityName = {ability.Config.AbilityName}");
                return null;
            }

            // ── 免疫 / 无控制受体：在创建实例之前拦下来 ──
            //
            // 必须放在下面的堆叠分支之前。那几条分支里有 Refresh / Prolong / 满层 SetDuration，
            // 只要放过任意一条，免疫期间就会"续上"已有控制的倒计时 ——
            // 表现为免控结束的瞬间立刻被控，比完全不免控更容易被当成 bug。
            //
            // 只有带标记的施加才需要检：没挂 Tags 的 modifier 与免疫无关，不付这次查询成本。
            if (config.Tags != GameplayTags.None)
            {
                // 每次现取，不缓存：组件可能已经 Dispose（与 NotifyControlChanged 同一套写法）。
                var control = GetComponent<ControlComponent>();

                // 没有 ControlComponent 就没有标记的唯一聚合者：tagMask / tagStacks /
                // IsControlled / CanDo 全都不存在，挂上去也只是个读不出来的死状态。
                // 按"免控"处理，等价于拒绝全部带标记的施加。
                //
                // 这条必须与下面的免疫命中分开：无受体时根本没有免疫源，
                // 不能播抵抗表现、也不能执行 OnImmuneActions。
                if (control == null) return null;

                if (control.TryGetImmune(config.Tags, out var immune))
                {
                    // 次数扣减不在这里做 —— Mixin 不认识状态名，
                    // 「扣几次、扣哪个 key、扣到多少移除」全在 ConfigImmuneMixin.Actions 里配。
                    // 本行只负责：这一击被挡下了，跑它的 Actions（抵抗表现 / 扣次数 / 反击）。
                    immune.ExecuteOnImmune();
                    return null;
                }
            }

            string key = ability.Config.AbilityName + "_" + config.ModifierName;
            var list = modifierDictionary[key];
            if (list!=null && list.Count > 0)
            {
                // 处理堆叠
                switch (config.StackingType)
                {
                    case StackingType.Unique:
                    {
                        // 只能存在唯一一个
                        return null;
                    }
                    case StackingType.Multiple:
                    {
                        // 互相独立存在，每个实例算一层（层数由实例个数体现），上限见 HasStackRoom
                        if (HasStackRoom(config, list.Count))
                        {
                            var modifier = ActorModifier.Create(applierID, config, ability, this);
                            modifiers.AddLast(modifier);
                            modifierDictionary.Add(key, modifier);
                            modifier.AfterAdd();
                            NotifyControlChanged();
                            return modifier;
                        }

                        // 满层：不新建实例，但刷新已有时长。
                        // 眩晕这类上限 1 层的标记，第二次命中若什么都不做，玩家看到的是
                        // "明明打上了却毫无反应"，比刷新更糟。
                        // 无限时长（-1）不能走 SetDuration —— 那会建出一个负数计时器。
                        if (config.Duration > 0)
                        {
                            for (int i = 0; i < list.Count; ++i)
                            {
                                list[i].SetDuration(config.Duration);
                            }
                        }

                        return  null;
                    }
                    case StackingType.Stack:
                    {
                        // 单实例叠层：共用一份 Properties / 计时器，层数靠自增，效果按层放大
                        var target = list[0];
                        if (HasStackRoom(config, target.StackCount))
                        {
                            target.TryAddStack();
                        }
                        // 满层也要续时间：满了还继续命中却完全没反馈，会让玩家以为技能丢了
                        // 无限时长（-1）不能走 SetDuration —— 那会建出一个负数计时器
                        if (config.Duration >= 0) target.SetDuration(config.Duration);
                        NotifyControlChanged();
                        return null;
                    }
                    case StackingType.Refresh:
                    {
                        // 刷新已存在的modifier
                        for (int i = 0; i < list.Count; ++i)
                        {
                            list[i].SetDuration(config.Duration);
                        }

                        return  null;
                    }
                    case StackingType.Prolong:
                    {
                        // 延长已存在的modifier
                        for (int i = 0; i < list.Count; ++i)
                        {
                            list[i].AddDuration(config.Duration);
                        }

                        return null;
                    }
                }
            }
            else
            {
                var modifier = ActorModifier.Create(applierID, config, ability, this);
                modifiers.AddLast(modifier);
                modifierDictionary.Add(key, modifier);
                modifier.AfterAdd();
                NotifyControlChanged();
                return modifier;
            }

            return null;
        }

        public void RemoveModifier(ActorModifier modifier)
        {
            if (isDestroy || modifier == null) return;
            string key = modifier.Ability?.Config?.AbilityName + "_" + modifier.Config?.ModifierName;
            if (modifierDictionary.Contains(key, modifier))
            {
                modifier.BeforeRemove();
                modifiers.Remove(modifier);
                modifierDictionary.Remove(key, modifier);
                modifier.Dispose();
                NotifyControlChanged();
                return;
            }

            // key 拼不出来（所依附的 ability 先被移除，Config 已置 null）或已被摘过：
            // 兜底按实例找一次。否则这个 modifier 会一直留在 modifiers 里，
            // 它带的标记再也摘不掉，角色会永久被禁控
            if (TryRemoveByIdentity(modifier))
            {
                modifier.BeforeRemove();
                modifiers.Remove(modifier);
                modifier.Dispose();
                NotifyControlChanged();
            }
        }

        /// <summary>
        /// 按实例从 <see cref="modifierDictionary"/> 里摘除（key 不可用时的兜底）。
        /// </summary>
        private bool TryRemoveByIdentity(ActorModifier modifier)
        {
            foreach (var pair in modifierDictionary)
            {
                var list = pair.Value;
                if (list == null || !list.Remove(modifier)) continue;
                if (list.Count == 0) modifierDictionary.Remove(pair.Key);
                return true;
            }
            return false;
        }

        public void RemoveModifier(string ability, string modifier)
        {
            if (isDestroy) return;
            string key = ability + "_" + modifier;
            var list = modifierDictionary[key];
            if (list == null || list.Count == 0) return;
            if (list.Count == 1)
            {
                RemoveModifier(list[0]);
                return;
            }

            var snapshot = list.ToArray();
            for (int i = 0; i < snapshot.Length; i++)
            {
                RemoveModifier(snapshot[i]);
            }
        }

        /// <summary>
        /// 削减层数（<see cref="RemoveModifierByTag"/> 的运行时入口）。
        ///
        /// <b>只按标记定位</b>：遍历 <see cref="modifiers"/> 命中所有带该标记的实例 ——
        /// 与读侧 <c>TagStackValue</c> 走 <c>ControlComponent.GetTagStack</c> 的聚合口径
        /// 完全一致，跨来源、跨 ability 叠的层都能被消耗。
        ///
        /// <b>数字同步、结构延后</b>：调用 <see cref="ActorModifier.ReduceStack"/> 立即改
        /// StackCount / Properties，归零只登记 <c>IsPendingRemove</c>；帧末由
        /// <see cref="FlushPendingRemoves"/> 统一摘除。这样同帧后续 Action 读得到减后层数，
        /// 又不会在别人遍历 modifiers 链表时改集合。
        ///
        /// <b>一次通知</b>：循环里不逐实例 Rebuild，结束统一 <see cref="NotifyControlChanged"/>；
        /// 帧末批量摘除期间用 <c>suppressNotify</c> 把 N 次 RemoveModifier 的通知也合并成一次。
        ///
        /// count：Consume 时是要消耗的层数，Keep 时是要保留的层数，Clear 时被忽略。
        /// 返回实际减掉的层数，供调用方按消耗量结算（<c>ResultKey</c>）。
        /// </summary>
        public int ConsumeStacks(GameplayTags tags, int count, StackRemoveMode mode)
        {
            if (isDestroy || modifiers == null || tags == GameplayTags.None) return 0;

            var targets = CollectStackTargets(tags);
            if (targets.Count == 0) return 0;

            // 最早到期的层先掉：Multiple 每层各自倒计时，优先消耗快到期的才符合
            // 「自然衰减」的直觉；无限时长（TillTime<=0）排最后，永不被优先削掉。
            targets.Sort((a, b) => SortKey(a).CompareTo(SortKey(b)));

            int total = 0;
            for (int i = 0; i < targets.Count; i++) total += targets[i].StackCount;

            int left;
            switch (mode)
            {
                case StackRemoveMode.Clear:
                    left = int.MaxValue;
                    break;
                case StackRemoveMode.Keep:
                    left = Math.Max(0, total - Math.Max(0, count));
                    break;
                default:
                    left = Math.Max(0, count);
                    break;
            }
            if (left == 0) return 0;

            int removed = 0;
            for (int i = 0; i < targets.Count && left > 0; i++)
            {
                int used = targets[i].ReduceStack(left);
                if (used <= 0) continue; // 防御：不让 left 原地不动把循环拖死
                removed += used;
                left -= used;
            }

            if (removed > 0) NotifyControlChanged();
            return removed;
        }

        private static long SortKey(ActorModifier m)
        {
            return m.TillTime > 0 ? m.TillTime : long.MaxValue;
        }

        /// <summary>
        /// 收集减层目标：按标记遍历活跃链表（跨来源、跨 ability）。
        /// 0 层 / 已回收的实例一律排除，避免把待摘除对象再算一遍。
        /// </summary>
        private List<ActorModifier> CollectStackTargets(GameplayTags tags)
        {
            var result = new List<ActorModifier>();
            for (var node = modifiers.First; node != null; node = node.Next)
            {
                var m = node.Value;
                if (m == null || m.IsDispose || m.Config == null || m.StackCount <= 0) continue;
                if ((m.Config.Tags & tags) == GameplayTags.None) continue;
                result.Add(m);
            }
            return result;
        }

        /// <summary>
        /// 登记一次帧末摘除。同一帧里多个来源同时减层只调度一次，
        /// 且登记不持有任何 modifier 引用 —— 宿主回池也不会被协程解引用。
        /// </summary>
        public void ScheduleFlushPendingRemoves()
        {
            if (isDestroy || flushScheduled) return;
            flushScheduled = true;
            FlushPendingRemovesAsync(this).Coroutine();
        }

        private static async ETTask FlushPendingRemovesAsync(AbilityComponent ac)
        {
            await UnityLifeTimeHelper.WaitUpdateFinish();
            ac.flushScheduled = false;
            if (!ac.isDestroy) ac.FlushPendingRemoves();
        }

        /// <summary>
        /// 帧末统一摘除本帧被减到 0 层的实例。
        /// 摘除期间挂起通知，结束后只重算一次标记聚合 —— 否则 Multiple 清 5 层
        /// 会触发 5 次全量 Rebuild、5 次 EntityTagsChanged 广播与 5 个中间态。
        /// </summary>
        private void FlushPendingRemoves()
        {
            if (modifiers == null) return;

            List<ActorModifier> pending = null;
            for (var node = modifiers.First; node != null; node = node.Next)
            {
                var m = node.Value;
                if (m != null && !m.IsDispose && m.IsPendingRemove)
                {
                    (pending ??= new List<ActorModifier>()).Add(m);
                }
            }
            if (pending == null) return;

            suppressNotify = true;
            try
            {
                for (int i = 0; i < pending.Count; i++)
                {
                    pending[i].FlushPendingRemove();
                }
            }
            finally
            {
                suppressNotify = false;
            }
            NotifyControlChanged();
        }

        public void RemoveAbility(ActorAbility ability)
        {
            if (isDestroy) return;
            if (abilities.Contains(ability))
            {
                ability.BeforeRemove();
                abilities.Remove(ability);
                ability.Dispose();
            }
        }
        public void RemoveAbility(string abilityName)
        {
            if (isDestroy) return;
            ActorAbility ability = null;
            foreach (var item in abilities)
            {
                if (item.Config.AbilityName == abilityName)
                {
                    ability = item;
                    break;
                }
            }
            if(ability==null) return;
            if (abilities.Contains(ability))
            {
                ability.BeforeRemove();
                abilities.Remove(ability);
                ability.Dispose();
            }
        }
        /// <summary>
        /// 按名字查已有实例（只读）。外部注入前必须先查：
        /// <see cref="AddAbility"/> 不去重，同名 Add 两次会得到两个 ActorAbility，效果翻倍。
        /// </summary>
        public ActorAbility GetAbility(string abilityName)
        {
            if (abilities == null) return null;
            for (var node = abilities.First; node != null; node = node.Next)
            {
                var a = node.Value;
                if (a?.Config != null && a.Config.AbilityName == abilityName) return a;
            }
            return null;
        }

        /// <summary>
        /// modifier 增删后通知控制仲裁者重算能力位掩码。
        /// 组件销毁过程中不通知：此时 isDestroy 已置位，销毁链路里的 RemoveModifier 也会提前返回，
        /// 没必要再去碰一个可能已经 Dispose 的 ControlComponent。
        /// </summary>
        /// <summary>
        /// 还能不能再加一层/一个实例。两道闸取交集，任一不满足就返回 false：
        ///
        /// 1. <b>标记池余量</b>（<see cref="ControlComponent.GetTagRoom"/>）：上限定义在
        ///    <see cref="GameplayTagMeta"/> 里、属于标记而不是 modifier —— 否则两个不同来源挂同一个
        ///    标记时会各按各的上限封顶（5 和 3），聚合后变成 8，"灼烧最多 5 层"就表达不出来；
        /// 2. <c>StackLimitCount</c>：modifier 自己的实例上限（0 = 不限），留给"没挂标记"的场合。
        ///
        /// <paramref name="current"/> 传"这个 modifier 现在已经占了几层"：
        /// Multiple 传实例个数，Stack 传 <see cref="ActorModifier.StackCount"/>。
        ///
        /// 没挂标记时只看第 2 条 —— 存量配置（Tags 全空）的行为与改动前完全一致。
        /// </summary>
        private bool HasStackRoom(ConfigAbilityModifier config, int current)
        {
            if (config.StackLimitCount > 0 && current >= config.StackLimitCount) return false;
            if (config.Tags == GameplayTags.None) return true;

            var control = GetComponent<ControlComponent>();
            return control == null || control.GetTagRoom(config.Tags) > 0;
        }

        // 按标记清状态统一走 ConsumeStacks(tags, 0, Clear) + ScheduleFlushPendingRemoves()：
        // 数字同步生效、帧末批量摘除，不再另开一条 RemoveModifiersByTag 链路。

        /// <summary>
        /// modifier 增删 / 层数变化后重算标记聚合与控制掩码。
        ///
        /// 加层走 <c>ApplyModifier</c> 的 Stack 分支，减层走 <c>ConsumeStacks</c>，
        /// 两条链各自在结尾通知一次；否则 <c>tagStacks</c> 还记着旧层数，
        /// 「灼烧 5 层上限」在引爆消耗层数后会表现为"层减了但池子没释放"。
        ///
        /// 帧末批量摘除期间 <c>suppressNotify</c> 为真，N 次 RemoveModifier 的通知
        /// 被合并成 Flush 结束后的一次。
        /// </summary>
        private void NotifyControlChanged()
        {
            if (isDestroy || suppressNotify) return;
            GetComponent<ControlComponent>()?.NotifyModifiersChanged();
        }

        public void ExecuteAbility(string ability)
        {
            foreach (var item in abilities)
            {
                if (item.Config.AbilityName == ability)
                {
                    Messager.Instance.Broadcast(Id,MessageId.ExecuteAbility,ability);
                    item.Execute();
                }
            }
        }

        #region 挂起的恢复型补执行

        /// <summary>
        /// 登记一次延迟补执行，返回用于取消的令牌（会被 <see cref="FlushPendingRestores"/> 提前兑现）。
        /// 同名重复登记时先撤掉旧的，保证同一能力最多只有一份延迟任务。
        /// </summary>
        public ETCancellationToken RegisterPendingRestore(string ability)
        {
            if (isDestroy || string.IsNullOrEmpty(ability)) return null;

            if (pendingRestores == null)
            {
                pendingRestores = new Dictionary<string, ETCancellationToken>();
            }
            else if (pendingRestores.TryGetValue(ability, out var old))
            {
                pendingRestores.Remove(ability);
                old.Cancel();
            }

            var token = new ETCancellationToken();
            pendingRestores[ability] = token;
            return token;
        }

        /// <summary>延迟到点后正常结算，把登记项摘掉（避免下次误兑现）。</summary>
        public void CompletePendingRestore(string ability, ETCancellationToken token)
        {
            if (pendingRestores == null || token == null) return;
            if (pendingRestores.TryGetValue(ability, out var cur) && ReferenceEquals(cur, token))
            {
                pendingRestores.Remove(ability);
            }
        }

        /// <summary>
        /// 立刻兑现所有挂起的恢复型补执行，并让原来的延迟协程自行退出。
        /// 由 <c>EquipHoldComponent</c> 在聚合显隐落到「收剑态」时调用 —— 那里是唯一
        /// 知道多 FSM 聚合结果的地方，比在各个打断来源上逐处打点更可靠。
        /// </summary>
        public void FlushPendingRestores()
        {
            if (pendingRestores == null || pendingRestores.Count == 0) return;

            // 先摘表再 Cancel：Cancel 会同步唤醒等待中的协程，协程回来时会调
            // CompletePendingRestore，若此时还在遍历原字典就会抛「集合被修改」。
            var names = new List<string>(pendingRestores.Count);
            var tokens = new List<ETCancellationToken>(pendingRestores.Count);
            foreach (var kv in pendingRestores)
            {
                names.Add(kv.Key);
                tokens.Add(kv.Value);
            }
            pendingRestores.Clear();

            for (int i = 0; i < names.Count; i++)
            {
                tokens[i]?.Cancel();
                ExecuteAbility(names[i]);
            }
        }

        /// <summary>析构前丢弃所有挂起项：只取消，不再补执行。</summary>
        private void DropPendingRestores()
        {
            if (pendingRestores == null) return;

            var tokens = new List<ETCancellationToken>(pendingRestores.Values);
            pendingRestores.Clear();
            pendingRestores = null;
            for (int i = 0; i < tokens.Count; i++)
            {
                tokens[i]?.Cancel();
            }
        }

        #endregion
    }
}