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
        
        private Zone defendArea;
        public static AIDefendAreaKnowledge Create(ConfigAIBeta config, Vector3 bornPos, Zone defendArea)
        {
            AIDefendAreaKnowledge res = ObjectPool.Instance.Fetch<AIDefendAreaKnowledge>();
            res.Config = config.DefendArea;
            res.defendArea = defendArea;
            var defendRange = res.Config.DefendRange;
            res.defendCenter = defendArea == null ? bornPos : defendArea.GetCenter();
            res.sqlDefendRange = defendRange * defendRange;
            return res;
        }

        public void Dispose()
        {
            Config = null;
            sqlDefendRange = 0;
            defendCenter = Vector3.zero;
            IsInDefendRange = false;
            defendArea = null;
        }

        public bool CheckInDefendArea(Vector3 point)
        {
            if (Config.Enable)
            {
                if (defendArea != null && !defendArea.IsDispose) return defendArea.GetSqrDistance(point) <= sqlDefendRange;
                return Vector3.SqrMagnitude(point - defendCenter) <= sqlDefendRange;
            }
            return true;
        }
    }
}