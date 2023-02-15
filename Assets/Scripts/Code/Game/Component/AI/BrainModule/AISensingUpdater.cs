using System.Collections.Generic;

namespace TaoTie
{
    /// <summary>
    /// 感知模块
    /// </summary>
    public class AISensingUpdater: BrainModuleBase
    {
        private AIComponent aiComponent;
        private Dictionary<uint, SensibleInfo> _enemySensibles; // 0x30
        private Dictionary<uint, SensibleInfo> _enemySensiblesPreparation;
        
        public AISensingUpdater(AIComponent aiComponent)
        {
            this.aiComponent = aiComponent;
        }
    }
}