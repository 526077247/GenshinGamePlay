using System;
using UnityEngine;

namespace TaoTie
{
    public class AIDefendAreaKnowledge: IDisposable
    {
        public ConfigAIDefendArea Config;
        private Vector3 defendCenter;
        private float sqlDefendRange;
        
        public bool IsInDefendRange;
        
        //public SimplePolygon defendArea;// todo: 先用球
        public static AIDefendAreaKnowledge Create(ConfigAIBeta config,Vector3 bornpos)
        {
            AIDefendAreaKnowledge res = ObjectPool.Instance.Fetch<AIDefendAreaKnowledge>();
            res.Config = config.DefendArea;
            var defendRange = res.Config.DefendRange;
            res.defendCenter = bornpos;
            res.sqlDefendRange = defendRange * defendRange;
            return res;
        }

        public void Dispose()
        {
            Config = null;
            sqlDefendRange = 0;
            defendCenter = Vector3.zero;
            IsInDefendRange = false;
        }

        public bool CheckInDefendArea(Vector3 point)
        {
            if (Config.Enable)
            {
                return Vector3.SqrMagnitude(point - defendCenter) < sqlDefendRange;
            }
            return true;
        }
    }
}