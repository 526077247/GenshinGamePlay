using System;

namespace TaoTie
{
    public class AIThreatKnowledge: IDisposable
    {
        private ConfigAIThreatSetting config;

        public void Dispose()
        {
            config = null;
        }
    }
}