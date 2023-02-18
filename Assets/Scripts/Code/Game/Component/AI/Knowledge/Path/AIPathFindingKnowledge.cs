using System;
using System.Collections.Generic;
using UnityEngine;

namespace TaoTie
{
    public class AIPathFindingKnowledge: IDisposable
    {
        public ListComponent<Vector3> Route;
        public PathFindingType Type;
        public string NavMeshAgentName;

        public static AIPathFindingKnowledge Create(ConfigAIBeta config)
        {
            AIPathFindingKnowledge res = ObjectPool.Instance.Fetch<AIPathFindingKnowledge>();
            res.Type = config.Path.Type;
            res.NavMeshAgentName = config.Path.NavMeshAgentName;
            return res;
        }
        public void Dispose()
        {
            Type = default;
            NavMeshAgentName = null;
            Route.Dispose();
            Route = null;
        }
    }
}