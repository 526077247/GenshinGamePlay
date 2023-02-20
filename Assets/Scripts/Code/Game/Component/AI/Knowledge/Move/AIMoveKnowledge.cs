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
    }
}