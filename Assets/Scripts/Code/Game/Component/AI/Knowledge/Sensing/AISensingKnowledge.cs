using System;
using System.Collections.Generic;

namespace TaoTie
{
    public class AISensingKnowledge: IDisposable
    {
        public ConfigAISensingSetting setting;
        public ConfigAISensing sensing;
        
        public Dictionary<long, SensibleInfo> enemySensibles;
        //用于决策选取的敌人，只选最近的
        public float nearestEnemyDistance;
        public long nearestEnemy;
        public bool scared;
        public void Dispose()
        {
            
        }
    }
}