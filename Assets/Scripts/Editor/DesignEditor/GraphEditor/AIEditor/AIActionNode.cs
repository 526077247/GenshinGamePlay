
using DaGenGraph;
using DaGenGraph.Editor;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    public class AIActionNode:Node
    {
        [HideReferenceObjectPicker]
        public DecisionActionNode Data = new DecisionActionNode();
        public override NodeView GetNodeView()
        {
            return new AINodeView();
        }

        public override void InitNode(Graph graph, Vector2 pos, string _name, int minimumInputPortsCount = 1, int minimumOutputPortsCount = 0)
        {
            base.InitNode(graph, pos, _name, minimumInputPortsCount, minimumOutputPortsCount);
            SetName("Action");
            
        }

        public override void AddDefaultPorts()
        {
            AddInputPort(EdgeMode.Override, false, false);
        }
    }
}