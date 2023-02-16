using System.Collections.Generic;

namespace TaoTie
{
    /// <summary>
    /// 感知模块
    /// </summary>
    public class AISensingUpdater : BrainModuleBase
    {
        private AIComponent aiComponent;
        private Dictionary<uint, SensibleInfo> enemySensibles;
        private Dictionary<uint, SensibleInfo> enemySensiblesPreparation;

        private AISensingKnowledge sensingKnowledge;

        public AISensingUpdater(AIComponent aiComponent)
        {
            this.aiComponent = aiComponent;
        }

        protected override void InitInternal()
        {
            base.InitInternal();
            sensingKnowledge = aiKnowledge.sensingKnowledge;
        }


        protected override void ClearInternal()
        {
            base.ClearInternal();
            sensingKnowledge = null;
            aiComponent = null;
            enemySensibles = null;
            enemySensiblesPreparation = null;
        }

        protected override void UpdateMainThreadInternal()
        {
            base.UpdateMainThreadInternal();
            CollectEnemies();
            ProcessEnemies();
        }

        private void CollectEnemies()
        {
        }

        private void ProcessEnemies()
        {
        }
    }
}