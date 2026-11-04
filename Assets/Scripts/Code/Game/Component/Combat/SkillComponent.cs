using System;
using System.Collections.Generic;
using UnityEngine;

namespace TaoTie
{
    public class SkillComponent:Component,IComponent,IComponent<ConfigSkill>
    {
        private NumericComponent numericComponent => parent.GetComponent<NumericComponent>();
        private CombatComponent combatComponent => parent.GetComponent<CombatComponent>();
        private MoveComponent moveComponent => parent.GetComponent<MoveComponent>();
        private AbilityComponent abilityComponent => parent.GetComponent<AbilityComponent>();

        /// <summary>
        /// 吟唱时长在 ability 里的变量名。施法时用 SetSpecials 写进去，
        /// 由该 ability 的 DelayDoAction.Delay（AbilityValue）读走当倒计时。
        /// </summary>
        private const string CastTimeSpecial = "CastTime";

        /// <summary>
        /// 施法序号。每次施法自增并写进技能 ability 的 special，
        /// DelayDoAction 到期时比对，不一致说明这笔延迟属于上一次施法，丢弃。
        /// 常驻 ability 不能靠"移除实例"来终止上一轮协程，只能靠这个。
        /// </summary>
        private const string CastSeqSpecial = "CastSeq";

        private int castSeq;

        public Dictionary<int, SkillInfo> SkillInfoMap;

        /// <summary>
        /// 槽位号（1..4）→ 当前装配的技能。定长数组而不是字典：槽位本来就是连续的，
        /// 数组把"只能有 Count 个"这条约束变成结构性的，写错的槽位号会立刻越界而不是静默查不到。
        ///
        /// 下标 0 永久弃用 —— 让 <see cref="SkillInfo.Slot"/> 的 0 干净地表示"不在任何槽位"
        /// （对象池回收后归零也是 0，两者同义，不用另造 -1 哨兵）。
        /// </summary>
        private SkillInfo[] slots;

        private int sprintBSId;
        public event Action<int> OnDoSkillEvt;
        #region IComponent

        public void Init()
        {
            Init(null);
        }
        public void Init(ConfigSkill config)
        {
            SkillInfoMap = new Dictionary<int, SkillInfo>();
            slots = new SkillInfo[GameConst.SkillSlotConst + 1];
            for (int i = 0; i < (config?.Skills?.Length ?? 0); i++)
            {
                if (config.Skills[i] == 0) continue;
                if (i < GameConst.SkillSlotConst)
                {
                    SetSkillInfo(config.Skills[i], i + 1);
                }
                else
                {
                    AddSkillInfo(config.Skills[i]);
                }
            }
            if (config != null && config.SprintBS != 0)
            {
                sprintBSId = config.SprintBS;
                AddSkillInfo(config.SprintBS);
            }
            Messager.Instance.AddListener<string>(Id,MessageId.ExecuteAbility,OnExecuteAbilityEvt);
        }
        public void Destroy()
        {
            Messager.Instance.RemoveListener<string>(Id,MessageId.ExecuteAbility,OnExecuteAbilityEvt);
            foreach (var item in SkillInfoMap)
            {
                item.Value.Dispose();
            }

            SkillInfoMap = null;
            slots = null;
            sprintBSId = 0;
        }
        

        private void OnExecuteAbilityEvt(string name)
        {
            foreach (var item in SkillInfoMap)
            {
                if (item.Value.TriggerCDType == (int)TriggerCDType.OnExecuteAbility
                    && name == item.Value.AbilityName)
                {
                    TriggerSkillCD(item.Value.ConfigId);
                }
            }
        }

        #endregion
        
        public void TryDoSkill(int configId)
        {
            if (SkillInfoMap == null || !SkillInfoMap.TryGetValue(configId, out var info)) return;
            DoSkill(info);
        }

        /// <summary>
        /// 按槽位号放技能 —— 按键入口（1..4 = 左键/Q/E/R）。
        ///
        /// 槽位号直接来自 <c>TryDoSkill.Slot</c> 配置，不再有任何"角色内唯一 id"的中间编号：
        /// 心法装配会改写 <see cref="slots"/>，按键读的是"这一格现在装的是什么"。
        /// </summary>
        public void TryDoSkillBySlot(int slot)
        {
            DoSkill(GetSlot(slot));
        }

        /// <summary>
        /// 取某槽位上的技能。槽位号非法或空槽一律返回 null，不抛异常 ——
        /// 内部编码错误（负数/越界）与配置写错（Slot=0 或超出 Count）走同一条容错路径。
        /// </summary>
        private SkillInfo GetSlot(int slot)
        {
            if (slots == null || slot <= 0 || slot >= slots.Length) return null;
            return slots[slot];
        }

        /// <summary>
        /// 施法的公共前半段。两个入口只差"怎么查到 SkillInfo"，其余（CD 判定、选敌转向、
        /// 广播、写 FSM）完全相同，合并成一处避免两条路径慢慢跑偏。
        /// </summary>
        private void DoSkill(SkillInfo info)
        {
            if (info == null) return;
            if (combatComponent == null) return;
            if (GetComponent<ControlComponent>() is { } ctl && !ctl.CanDo(ControlFlag.CastSkill)) return;

            var configId = info.ConfigId;
            if (IsSkillInCD(configId)) return;

            if (info.TriggerCDType == (int) TriggerCDType.OnSpell)
                TriggerSkillCD(configId);
            combatComponent.SelectAttackTarget(true);
            var target = combatComponent.GetAttackTarget();
            if (target is SceneEntity se && moveComponent != null)
            {
                moveComponent.ForceLookAt(se.Position);
            }
            OnDoSkillEvt?.Invoke(configId);
            TryEnterFsm(info);
        }

        /// <summary>
        /// 施法入口：先给技能本体的 ability 注入本等级参数，再统一写 FSM 参数。
        ///
        /// <b>吟唱不再由任何 Mono 组件计时，也不再单独挂一个"吟唱壳"</b>。
        /// 技能 ability（<see cref="SkillInfo.AbilityName"/>）本身就是吟唱载体：
        /// 它常驻在身上，里面有 <c>ConfigDoActionByStateIDMixin</c> 监听吟唱三态，
        /// 进起手时启动 DelayDoAction，到期执行自己（写伤害 + 写 CD）并写 FSM 的 CastDone。
        /// 这里只做两件事：
        /// <list type="number">
        /// <item>把本等级的 CastTime / 施法序号塞进 ability 的 special；</item>
        /// <item>写 UseSkill + SkillId，让 FSM 进吟唱姿势。</item>
        /// </list>
        /// 计时只在 FSM 真的进了吟唱状态之后才开始，所以第 1 步早于切状态也没关系。
        ///
        /// 打断不结算靠 ability 里的 RequireModifier（Casting）：状态一跳走 modifier 就被摘，
        /// Delay 到期发现它不在了就放弃 —— 常驻 ability 的 IsDispose 永远是 false，指望不上。
        ///
        /// 都先消音按住的方向键，否则边跑边放会被自己的移动意图顶掉。
        /// </summary>
        private void TryEnterFsm(SkillInfo info)
        {
            GetComponent<LocalInputController>()?.SuppressMoveUntilRelease();

            if (info.CastTime > 0)
            {
                if (info.SkillId <= 0)
                {
                    // 没有 FSM 动作 id 就进不了吟唱姿势，ability 里的 mixin 也不会开始计时
                    Log.Error($"[Skill] {info.ConfigId} 配了 CastTime 但 FsmSkillId<=0，吟唱无法进 FSM，已按瞬发处理");
                }
                else if (string.IsNullOrEmpty(info.AbilityName))
                {
                    Log.Error($"[Skill] {info.ConfigId} 配了 CastTime 但 AbilityName 为空，吟唱不会计时也不会结算");
                }
                else
                {
                    BeginCast(info);
                }
            }

            // 有 FSM 动作 id 就交给状态机（吟唱/瞬发都走这条）
            if (info.SkillId > 0)
            {
                combatComponent.UseSkillImmediately(info.SkillId);
                return;
            }

            // 既没吟唱也没 FSM 输入（CastTime 配成 0、SkillConfig.FsmSkillId 也是 0）：
            // 直接结算，否则这个技能按下去什么都不会发生
            if (!string.IsNullOrEmpty(info.AbilityName))
            {
                abilityComponent?.ExecuteAbility(info.AbilityName);
            }
        }

        /// <summary>
        /// 给技能 ability 注入本次施法的参数。
        ///
        /// ability 是常驻的（<see cref="SkillLoadoutComponent"/> 装配时挂上），所以这里
        /// <b>不能</b> AddAbility 第二份 —— 那会让伤害 mixin 叠成两份。
        /// 取不到实例才补挂（怪物 AI / 临时注入没走装配套路的情况）。
        /// </summary>
        private void BeginCast(SkillInfo info)
        {
            if (abilityComponent == null) return;

            var ability = abilityComponent.GetAbility(info.AbilityName);
            if (ability == null)
            {
                // Get() 是字典下标，名字不存在直接抛异常；这里用 TryGetValue 兜住，只报错不中断施法
                if (!ConfigAbilityCategory.Instance.GetAll().TryGetValue(info.AbilityName, out var conf) || conf == null)
                {
                    Log.Error($"[Skill] {info.ConfigId} 的 ability 不存在：{info.AbilityName}");
                    return;
                }
                ability = abilityComponent.AddAbility(conf);
            }
            if (ability == null) return;

            ability.SetSpecials(CastTimeSpecial, info.CastTime);
            // 序号必须最后写：DelayDoAction 在 EnterActions 里读到的是本次序号，
            // 上一轮还没到期的协程醒来发现序号变了就会自己退出
            ability.SetSpecials(CastSeqSpecial, ++castSeq);
        }
        /// <summary>
        /// 批量注入技能（场景模板 / 外部系统用）。参数是 <see cref="SkillConfig.Id"/> 数组。
        ///
        /// 注入的技能<b>不占槽位</b>（<see cref="SkillInfo.Slot"/> = 0）：它们没有按键入口，
        /// 只按 ConfigId 查（怪物 AI 走 <see cref="TryDoSkill"/>）。按键槽位只有心法装配会写。
        ///
        /// 走 <see cref="SetSkillInfo"/> 而不是 <see cref="AddSkillInfo"/>：后者遇到同 ConfigId 会
        /// 直接复用旧实例，重复注入同一份配置等于什么都没发生；SetSkillInfo 先清同槽位 / 同 ConfigId
        /// 再注册，既不翻倍、也不会留下指向已 Dispose 实例的映射。
        /// </summary>
        public void InjectSkills(int[] configIds)
        {
            if (configIds == null || SkillInfoMap == null) return;
            for (int i = 0; i < configIds.Length; i++)
            {
                if (configIds[i] == 0) continue;
                SetSkillInfo(configIds[i], 0);
            }
        }

        /// <summary>
        /// 技能表里到底有没有这一项。
        ///
        /// <see cref="IsSkillInCD"/> 对<b>未知</b>技能返回 true（视为永远在 CD），这是「查不到就别放」
        /// 的安全口径，但也让「配置里少了一条技能」和「技能正在 CD」表现完全一样。
        /// 需要区分这两者的地方（AI 诊断）先用这个方法问一句。
        /// </summary>
        public bool HasSkill(int configId)
        {
            return SkillInfoMap != null && SkillInfoMap.ContainsKey(configId);
        }

        public bool IsSkillInCD(int configId)
        {
            if (SkillInfoMap.TryGetValue(configId, out var data))
            {
                var timeNow =  GameTimerManager.Instance.GetTimeNow();
                return timeNow < data.LastSpellTime + data.CD.GetData(numericComponent);
            }
            return true;
        }
        /// <summary>
        /// 冲刺是否CD
        /// </summary>
        /// <returns></returns>
        public bool IsSprintBSInCD()
        {
            if (sprintBSId == 0) return false;
            return IsSkillInCD(sprintBSId);
        }

        /// <summary>
        /// 注册一条技能（不带槽位）。用于角色自带技能与场景模板注入 —— 它们没有按键入口，
        /// 只按 ConfigId 查；按键槽位由心法装配走 <see cref="SetSkillInfo"/> 写入。
        ///
        /// 注意同 <paramref name="configId"/> 已存在时直接复用旧实例、不新建 ——
        /// 这是"重复注入同一个技能不翻倍"的保证，也是它<b>不能</b>用来装配槽位的原因。
        /// </summary>
        public SkillInfo AddSkillInfo(int configId)
        {
            if (configId == 0 || SkillInfoMap == null) return null;
            if (!SkillInfoMap.TryGetValue(configId, out var res))
            {
                SkillInfoMap[configId] = res = SkillInfo.Create(configId, 0);
            }

            return res;
        }

        /// <summary>
        /// 按槽位卸下（心法重新装配时用）。槽位上的技能必然在 <see cref="SkillInfoMap"/> 里，
        /// 统一走 <see cref="RemoveSkillInfo"/> 清两处，避免只清数组、留下悬空的 ConfigId 映射。
        /// </summary>
        public void RemoveSkillBySlot(int slot)
        {
            var info = GetSlot(slot);
            if (info != null) RemoveSkillInfo(info.ConfigId);
        }

        /// <summary>按 ConfigId 移除：字典、槽位数组、实例一起清</summary>
        public void RemoveSkillInfo(int configId)
        {
            if (SkillInfoMap == null) return;
            if (!SkillInfoMap.TryGetValue(configId, out var info) || info == null) return;

            SkillInfoMap.Remove(configId);
            // 引用相等保护：槽位可能已经换人，只有"确实指向这个实例"才置空，
            // 否则会把刚装进同一格的新技能从槽位上摘掉
            if (slots != null && info.Slot > 0 && info.Slot < slots.Length && slots[info.Slot] == info)
            {
                slots[info.Slot] = null;
            }

            info.Dispose();
        }

        /// <summary>
        /// 原子装配：先清同槽位、再清同 ConfigId，最后注册。心法装配与模板注入共用这一个入口。
        ///
        /// <paramref name="slot"/> 传 0 表示不占槽位（模板注入）；&gt;0 时必须是 1..<see cref="GameConst.SkillSlotConst"/>，
        /// 越界会报错并降级成"不占槽位"，不抛异常 —— 配置写错不该让整场战斗崩掉。
        /// <paramref name="skillIdOverride"/> 传 -1 表示用 <see cref="SkillConfig.FsmSkillId"/>；
        /// 传 0 表示显式不进 FSM；&gt;0 是心法等级表指定的动作 id。
        /// </summary>
        public SkillInfo SetSkillInfo(int configId, int slot, string cdOverride = null,
            int triggerOverride = -1, string abilityName = null, int castTimeOverride = -1,
            int skillIdOverride = -1)
        {
            if (configId == 0 || SkillInfoMap == null) return null;
            if (slot > 0 && (slots == null || slot >= slots.Length))
            {
                Log.Error($"[Skill] 槽位号越界 {slot}（合法 1..{GameConst.SkillSlotConst}），{configId} 降级为不占槽位");
                slot = 0;
            }

            // 先清槽位（可能装着别的技能），再清同 ConfigId（可能占着别的槽位），最后建新的
            if (slot > 0) RemoveSkillBySlot(slot);
            RemoveSkillInfo(configId);

            var res = SkillInfo.Create(configId, slot);
            res.SetRuntime(cdOverride, triggerOverride, abilityName, castTimeOverride, skillIdOverride);
            SkillInfoMap[configId] = res;
            if (slot > 0) slots[slot] = res;
            return res;
        }

        public void TriggerSkillCD(int configId)
        {
            if (SkillInfoMap.TryGetValue(configId, out var info))
            {
                info.LastSpellTime = GameTimerManager.Instance.GetTimeNow();
            }
        }
        
        public void TriggerSprintBSCD()
        {
            if (sprintBSId == 0) return;
            TriggerSkillCD(sprintBSId);
        }
    }
}