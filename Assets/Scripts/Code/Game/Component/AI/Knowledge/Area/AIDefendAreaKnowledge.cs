using System;
using UnityEngine;

namespace TaoTie
{
    public class AIDefendAreaKnowledge: IDisposable
    {
        public ConfigAIDefendArea config;
        public float defendRange;
        public Vector3 defendCenter;
        public bool isInDefendRange;

        public static AIDefendAreaKnowledge Create(ConfigAIBeta config,Vector3 bornpos)
        {
            AIDefendAreaKnowledge res = ObjectPool.Instance.Fetch<AIDefendAreaKnowledge>();
            res.config = config.DefendArea;
            res.defendRange = res.config.DefendRange;
            res.defendCenter = bornpos;
            return res;
        }

        public void Dispose()
        {
            config = null;
            defendRange = 0;
            defendCenter = Vector3.zero;
            isInDefendRange = false;
        }
    }
}