using System;

namespace TaoTie
{
    public class SkillInfo: IDisposable
    {
        public int SkillId{ get; private set; }
        public ConfigAISkill Config { get; private set; }

        public float CD { get; private set; }
        
        /// <summary>
        /// 下一次可施法时间
        /// </summary>
        public long NextSpellTime;
        public static SkillInfo Create(ConfigAISkill skill)
        {
            SkillInfo res = ObjectPool.Instance.Fetch<SkillInfo>();
            res.Config = skill;
            res.SkillId = skill.SkillID;
            res.CD = skill.CD;
            return res;
        }
        
        public void Dispose()
        {
            Config = null;
            SkillId = 0;
            CD = 0;
            NextSpellTime = 0;
        }
    }
}