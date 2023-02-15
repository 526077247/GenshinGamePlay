using System;
using System.Collections.Generic;

namespace TaoTie
{
    public class AISensingKnowledge: IDisposable
    {
        public ConfigAISensingSetting setting;
        public Dictionary<int, ConfigAISensingSetting> templateInUse;
        
        public Dictionary<uint, SensibleInfo> enemySensibles;
        //用于决策选取的敌人，只选最近的
        public float nearestEnemyDistance;
        public uint nearestEnemy;
        public bool scared;
        public void Dispose()
        {
            
        }
    }
}