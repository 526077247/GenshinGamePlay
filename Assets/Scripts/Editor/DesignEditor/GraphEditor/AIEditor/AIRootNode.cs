using DaGenGraph;
using DaGenGraph.Editor;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    public class AIRootNode:Node
    {
        public DecisionArchetype Type; 
        public override NodeView GetNodeView()
        {
            return new AINodeView();
        }

        public override void InitNode(Graph graph, Vector2 pos, string _name, int minimumInputPortsCount = 1, int minimumOutputPortsCount = 0)
        {
            base.InitNode(graph, pos, _name, minimumInputPortsCount, minimumOutputPortsCount);
            SetName("Root");
            
        }

        public override void AddDefaultPorts()
        {
            AddOutputPort(EdgeMode.Override, false, false, "Root");
            AddOutputPort(EdgeMode.Override, false, false, "CombatRoot");
        }
    }
}