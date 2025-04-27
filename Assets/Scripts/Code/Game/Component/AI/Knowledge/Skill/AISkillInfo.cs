using System;
using Random = UnityEngine.Random;

namespace TaoTie
{
    public class AISkillInfo: IDisposable
    {
        public int ConfigId => Config.ConfigId;
        public ConfigAISkill Config { get; private set; }
        public SkillConfig SkillConfig => SkillConfigCategory.Instance.Get(ConfigId);
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

        public void TriggerCD(long currentTime)
        {
            NextAvailableUseTick = currentTime + CD;
            if (Config.CdUpperRange > 0)
            {
                NextAvailableUseTick += Random.Range(0, Config.CdUpperRange);
            }
        }
    }
}