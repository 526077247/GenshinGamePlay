using System;
using System.Collections.Generic;

namespace TaoTie
{
    public abstract class AITacticKnowledge<TacticConfig, TacticDataConfig>: IDisposable
        where TacticConfig : ConfigAITacticBaseSetting
    {
        protected AITacticCondition condition;
        
        public TacticConfig config { get; protected set; }
        public TacticDataConfig data { get; protected set;}
        
        protected abstract TacticDataConfig defaultSetting { get; }
        protected abstract Dictionary<int, TacticDataConfig> specifications { get; }

        public void LoadData(TacticConfig tacticConfig)
        {
            if (tacticConfig != null)
            {
                config = tacticConfig;
                condition = AITacticCondition.Create(config.Condition);
                data = defaultSetting;
            }
        }

        public void SwitchSetting(int poseID)
        {
            if (specifications!=null && specifications.TryGetValue(poseID, out var setting))
            {
                data = setting;
            }
        }
        public void Dispose()
        {
            condition.Dispose();
            condition = null;
            config = null;
            data = default;
        }

        public bool NerveCheck(AIKnowledge knowledge)
        {
            return condition.CheckPose(knowledge);
        }
    }
}