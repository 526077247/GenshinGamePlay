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

        public void Dispose()
        {
            config = null;
        }
    }
}