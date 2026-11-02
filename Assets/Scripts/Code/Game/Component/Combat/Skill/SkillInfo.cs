using System;
using System.Collections.Generic;

namespace TaoTie
{
    public class SkillInfo: IDisposable
    {
        public int ConfigId { get; private set; }
        public int SkillId { get; private set; }
        public ConfigSkillInfo Info;
        public SkillConfig SkillConfig => SkillConfigCategory.Instance.Get(ConfigId);
        
        /// <summary>空 = 用 SkillConfig.CD</summary>
        private string cdOverride;
        /// <summary>-1 = 用 SkillConfig.TriggerCDType</summary>
        private int triggerOverride = -1;
        /// <summary>空 = 用 SkillConfig.AbilityName</summary>
        private string abilityNameOverride;

        public int TriggerCDType => triggerOverride >= 0 ? triggerOverride : SkillConfig.TriggerCDType;
        public string AbilityName => string.IsNullOrEmpty(abilityNameOverride) ? SkillConfig.AbilityName : abilityNameOverride;

        public FormulaStringFx CD => FormulaStringFx.Get(string.IsNullOrEmpty(cdOverride) ? SkillConfig.CD : cdOverride);
        public long LastSpellTime;

        public void SetRuntime(string cd, int trigger, string ability)
        {
            cdOverride = cd;
            triggerOverride = trigger;
            abilityNameOverride = ability;
        }

        public static SkillInfo Create(ConfigSkillInfo info)
        {
            var res = ObjectPool.Instance.Fetch<SkillInfo>();
            res.ConfigId = info.ConfigId;
            res.SkillId = info.SkillID;
            res.Info = info;
            // 对象池复用：覆盖字段必须在这里重置，否则上一份心法的等级会泄漏到自带技能上
            res.cdOverride = null;
            res.triggerOverride = -1;
            res.abilityNameOverride = null;
            res.LastSpellTime = 0;
            return res;
        }

        public void Dispose()
        {
            SkillId = 0;
            ConfigId = 0;
            Info = null;
            cdOverride = null;
            triggerOverride = -1;
            abilityNameOverride = null;
            LastSpellTime = 0;
        }
    }
}