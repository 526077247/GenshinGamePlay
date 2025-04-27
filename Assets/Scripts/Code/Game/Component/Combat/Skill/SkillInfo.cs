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
        public FormulaStringFx CD => FormulaStringFx.Get(SkillConfig.CD);
        public long LastSpellTime;
        public static SkillInfo Create(ConfigSkillInfo info)
        {
            var res = ObjectPool.Instance.Fetch<SkillInfo>();
            res.ConfigId = info.ConfigId;
            res.SkillId = info.SkillID;
            res.Info = info;
            return res;
        }

        public void Dispose()
        {
            SkillId = 0;
            ConfigId = 0;
            Info = null;
        }
    }
}