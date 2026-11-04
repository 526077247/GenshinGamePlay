using System;

namespace TaoTie
{
    public class SkillInfo: IDisposable
    {
        public int ConfigId { get; private set; }

        /// <summary>
        /// 技能槽位号（1..4 = 左键/Q/E/R）。0 = 不在任何槽位（角色自带技能、怪物模板技能）。
        ///
        /// 槽位号同时是 <c>SkillComponent.slots</c> 的数组下标，所以必须落在 1..<see cref="GameConst.SkillSlotConst"/>；
        /// 0 号格永久弃用，只为让"无槽位"和对象池回收后的归零值同义。
        /// </summary>
        public int Slot { get; private set; }

        /// <summary>
        /// 进 FSM 时写的动作 id；0=不进 FSM。
        /// </summary>
        public int SkillId => skillIdOverride >= 0 ? skillIdOverride : (SkillConfig?.FsmSkillId ?? 0);

        public SkillConfig SkillConfig => SkillConfigCategory.Instance.Get(ConfigId);
        
        /// <summary>空 = 用 SkillConfig.CD</summary>
        private string cdOverride;
        /// <summary>-1 = 用 SkillConfig.TriggerCDType</summary>
        private int triggerOverride = -1;
        /// <summary>空 = 用 SkillConfig.AbilityName</summary>
        private string abilityNameOverride;
        /// <summary>-1 = 用 SkillConfig.CastTime</summary>
        private int castTimeOverride = -1;
        /// <summary>-1 = 用 SkillConfig.FsmSkillId；0 = 显式不进 FSM</summary>
        private int skillIdOverride = -1;

        public int TriggerCDType => triggerOverride >= 0 ? triggerOverride : SkillConfig.TriggerCDType;
        public string AbilityName => string.IsNullOrEmpty(abilityNameOverride) ? SkillConfig.AbilityName : abilityNameOverride;

        /// <summary>
        /// 吟唱时长(毫秒)，口径 = 进入起手 → 出伤害，<b>不含</b>收招段。&lt;=0 视为瞬发。
        /// 施法时通过 SetSpecials 注入技能 ability，由它的 DelayDoAction 计时。
        /// </summary>
        public int CastTime => castTimeOverride >= 0 ? castTimeOverride : SkillConfig.CastTime;
        public bool IsChant => CastTime > 0;

        public FormulaStringFx CD => FormulaStringFx.Get(string.IsNullOrEmpty(cdOverride) ? SkillConfig.CD : cdOverride);
        public long LastSpellTime;

        public void SetRuntime(string cd, int trigger, string ability,
            int castTime = -1, int skillId = -1)
        {
            cdOverride = cd;
            triggerOverride = trigger;
            abilityNameOverride = ability;
            castTimeOverride = castTime;
            skillIdOverride = skillId;
        }

        public static SkillInfo Create(int configId, int slot)
        {
            var res = ObjectPool.Instance.Fetch<SkillInfo>();
            res.ConfigId = configId;
            res.Slot = slot;
            // 对象池复用：覆盖字段必须在这里重置，否则上一份心法的等级会泄漏到自带技能上
            res.cdOverride = null;
            res.triggerOverride = -1;
            res.abilityNameOverride = null;
            res.castTimeOverride = -1;
            res.skillIdOverride = -1;
            res.LastSpellTime = 0;
            return res;
        }

        public void Dispose()
        {
            ConfigId = 0;
            Slot = 0;
            cdOverride = null;
            triggerOverride = -1;
            abilityNameOverride = null;
            castTimeOverride = -1;
            skillIdOverride = -1;
            LastSpellTime = 0;
        }
    }
}
