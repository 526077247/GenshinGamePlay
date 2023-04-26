using System;
using UnityEngine;

namespace TaoTie
{
    public class AIDefendAreaKnowledge: IDisposable
    {
        public ConfigAIDefendArea Config;
        public float DefendRange;
        public Vector3 DefendCenter;
        public bool IsInDefendRange;
        private float SqlDefendRange;
        public static AIDefendAreaKnowledge Create(ConfigAIBeta config,Vector3 bornpos)
        {
            AIDefendAreaKnowledge res = ObjectPool.Instance.Fetch<AIDefendAreaKnowledge>();
            res.Config = config.DefendArea;
            res.DefendRange = res.Config.DefendRange;
            res.DefendCenter = bornpos;
            res.SqlDefendRange = res.DefendRange * res.DefendRange;
            return res;
        }

        public void Dispose()
        {
            Config = null;
            DefendRange = 0;
            DefendCenter = Vector3.zero;
            IsInDefendRange = false;
        }

        public bool CheckInDefendArea(Vector3 point)
        {
            if (Config.Enable)
            {
                return Vector3.SqrMagnitude(point - DefendCenter) < SqlDefendRange;
            }
            return true;
        }
    }
}