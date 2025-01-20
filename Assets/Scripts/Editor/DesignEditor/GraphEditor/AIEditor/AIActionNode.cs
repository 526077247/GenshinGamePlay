
using DaGenGraph;
using DaGenGraph.Editor;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [NodeViewType(typeof(AIActionNodeView))]
    public class AIActionNode:JsonNodeBase
    {
        [HideReferenceObjectPicker]
        public DecisionActionNode Data = new DecisionActionNode();

        public override void InitNode(Vector2 pos, string nodeName, int minInputPortsCount = 0, int minOutputPortsCount = 0)
        {
            base.InitNode(pos, nodeName, minInputPortsCount, minOutputPortsCount);
            SetName("Action");
        }

        public override void AddDefaultPorts()
        {
            AddInputPort(EdgeMode.Override, false, false);
        }
    }
}