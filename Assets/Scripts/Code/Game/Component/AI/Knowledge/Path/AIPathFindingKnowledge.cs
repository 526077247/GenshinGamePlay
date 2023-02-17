using System;
using System.Collections.Generic;
using UnityEngine;

namespace TaoTie
{
    public class AIPathFindingKnowledge: IDisposable
    {
        public ListComponent<Vector3> route;
        public PathFindingType type;
        public string navMeshAgentName;
        public void Dispose()
        {
            route = null;
        }
    }
}