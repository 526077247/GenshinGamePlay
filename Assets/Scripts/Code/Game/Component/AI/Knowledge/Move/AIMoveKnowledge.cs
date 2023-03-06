using System;

namespace TaoTie
{
    public class AIMoveKnowledge :IDisposable
    {
        public ConfigAIMove config;
        public bool canFly;
        public bool disableMoveTactic;
        public bool inAir;
        public bool inWater;
        public bool canMove = true;
        public bool canTurn = true;
        public static AIMoveKnowledge Create(ConfigAIBeta config)
        {
            AIMoveKnowledge res = ObjectPool.Instance.Fetch<AIMoveKnowledge>();
            res.config = config.moveSetting;
            return res;
        }

        public void Dispose()
        {
            config = null;
        }
        
        public float GetAlmostReachDistance(AIMoveSpeedLevel speed)
        {
            if (speed == AIMoveSpeedLevel.Walk)
                return config.almostReachedDistanceWalk;
            else
                return config.almostReachedDistanceRun;
        }
    }
}