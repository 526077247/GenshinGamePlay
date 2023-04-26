using System;

namespace TaoTie
{
    public class AITacticCondition: IDisposable
    {
        private AIPoseSelector poseSelector;

        public static AITacticCondition Create(ConfigAITacticCondition condition)
        {
            AITacticCondition res = ObjectPool.Instance.Fetch<AITacticCondition>();
            res.poseSelector = AIPoseSelector.Create(condition?.PoseId);
            return res;
        }
        public void Dispose()
        {
            poseSelector.Dispose();
            poseSelector = null;
        }

        public bool CheckPose(AIKnowledge knowledge)
        {
            return poseSelector.CheckValidPose(knowledge.PoseID);
        }
    }
}