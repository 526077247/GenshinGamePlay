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
                // 不能消耗 ImmuneCount、播抵抗表现、执行 OnImmuneActions。
                if (control == null) return null;

                if (control.TryGetImmune(config.Tags, out var immune))
                {
                    bool alive = immune.Consume();

                    // Actions 必须在移除宿主之前跑：RemoveHost 会把本实例回收进对象池，
                    // 之后再访问它就是读一个已清空的壳。
                    immune.ExecuteOnImmune();

                    if (!alive) immune.RemoveHost();
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
        /// 按名字查已有实例（只读）。心法注入前必须先查：
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

        /// <summary>
        /// 清掉所有挂着指定标记的 modifier（净化 / 驱散 / 免疫触发后的清理）。
        /// 传组合值表示"命中任一标记就清"，返回清掉的条数。
        /// </summary>
        public int RemoveModifiersByTag(GameplayTags tags)
        {
            if (isDestroy || tags == GameplayTags.None || modifiers == null) return 0;

            List<ActorModifier> snapshot = null;
            for (var node = modifiers.First; node != null; node = node.Next)
            {
                var m = node.Value;
                if (m?.Config == null || (m.Config.Tags & tags) == GameplayTags.None) continue;
                (snapshot ??= new List<ActorModifier>()).Add(m);
            }
            if (snapshot == null) return 0;

            for (int i = 0; i < snapshot.Count; i++) RemoveModifier(snapshot[i]);
            return snapshot.Count;
        }

        private void NotifyControlChanged()
        {
            if (isDestroy) return;
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
    }
}