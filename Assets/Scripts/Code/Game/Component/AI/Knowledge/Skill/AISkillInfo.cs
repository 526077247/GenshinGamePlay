using System;

namespace TaoTie
{
    public class AISkillInfo: IDisposable
    {
        public int SkillId => Config.SkillID;
        public ConfigAISkill Config { get; private set; }

        public int CD => Config.CD;
        
        /// <summary>
        /// 下一次可施法时间
        /// </summary>
        public long NextAvailableUseTick;
        public static AISkillInfo Create(ConfigAISkill skill)
        {
            AISkillInfo res = ObjectPool.Instance.Fetch<AISkillInfo>();
            res.Config = skill;
            return res;
        }
        
        public void Dispose()
        {
            Config = null;
            NextAvailableUseTick = 0;
            ObjectPool.Instance.Recycle(this);
        }
    }
}