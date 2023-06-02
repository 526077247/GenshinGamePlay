using System;

namespace TaoTie
{
    public class AIMoveKnowledge :IDisposable
    {
        public ConfigAIMove Config;
        public bool DisableMoveTactic;
        public bool CanMove = true;
        public bool CanTurn = true;
        public static AIMoveKnowledge Create(ConfigAIBeta config)
        {
            AIMoveKnowledge res = ObjectPool.Instance.Fetch<AIMoveKnowledge>();
            res.Config = config.MoveSetting;
            return res;
        }

        public void Dispose()
        {
            Config = null;
        }
        
        public float GetAlmostReachDistance(AIMoveSpeedLevel speed)
        {
            if (speed == AIMoveSpeedLevel.Walk)
                return Config.AlmostReachedDistanceWalk;
            else
                return Config.AlmostReachedDistanceRun;
        }
    }
}