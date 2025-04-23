using System;
using System.Collections.Generic;

namespace TaoTie
{
    public class SkillInfo: IDisposable
    {
        public int SkillID { get; private set; }
        public SkillConfig SkillConfig => SkillConfigCategory.Instance.Get(SkillID);

        public static SkillInfo Create(int skillId)
        {
            var res = ObjectPool.Instance.Fetch<SkillInfo>();
            res.SkillID = skillId;
            return res;
        }

        public void Dispose()
        {
            SkillID = 0;
        }
    }
}