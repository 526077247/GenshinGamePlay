using System;
using System.Collections.Generic;

namespace TaoTie
{
    public abstract class AITacticKnowledge<TacticConfig, TacticDataConfig>: IDisposable
        where TacticConfig : ConfigAITacticBaseSetting
    {
        protected AITacticCondition condition;
        
        public TacticConfig Config { get; protected set; }
        public TacticDataConfig Data { get; protected set;}
        
        protected abstract TacticDataConfig defaultSetting { get; }
        protected abstract Dictionary<int, TacticDataConfig> specifications { get; }

        public void LoadData(TacticConfig tacticConfig)
        {
            if (tacticConfig != null)
            {
                Config = tacticConfig;
                condition = AITacticCondition.Create(Config.Condition);
                Data = defaultSetting;
            }
        }

        public void SwitchSetting(int poseID)
        {
            if (specifications!=null && specifications.TryGetValue(poseID, out var setting))
            {
                Data = setting;
            }
        }
        public void Dispose()
        {
            condition.Dispose();
            condition = null;
            Config = null;
            Data = default;
        }

        public bool NerveCheck(AIKnowledge knowledge)
        {
            return condition.CheckPose(knowledge);
        }
    }
}