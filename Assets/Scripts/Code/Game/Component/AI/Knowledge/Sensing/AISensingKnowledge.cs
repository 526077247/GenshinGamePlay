using System;
using System.Collections.Generic;

namespace TaoTie
{
    public class AISensingKnowledge: IDisposable
    {
        public ConfigAISensingSetting Setting;
        public Dictionary<string, ConfigAISensingSetting> TemplateInUse;

        public Dictionary<long, SensibleInfo> EnemySensibles;
        //用于决策选取的敌人，只选最近的
        public float NearestEnemyDistance;
        public long NearestEnemy;
        /// <summary>
        /// 是否害怕中
        /// </summary>
        public bool Scared;

        public static AISensingKnowledge Create(ConfigAIBeta config)
        {
            AISensingKnowledge res = ObjectPool.Instance.Fetch<AISensingKnowledge>();
            res.Setting = config.Sensing.Setting;
            res.TemplateInUse = config.Sensing.Settings;
            return res;
        }
        
        public void Dispose()
        {
            Setting = null;
            TemplateInUse = null;
            EnemySensibles = null;
            NearestEnemyDistance = 0;
            NearestEnemy = 0;
            Scared = false;
            ObjectPool.Instance.Recycle(this);
        }
    }
}