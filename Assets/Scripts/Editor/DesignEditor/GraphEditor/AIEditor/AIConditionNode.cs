using DaGenGraph;
using DaGenGraph.Editor;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [NodeViewType(typeof(AIConditionNodeView))]
    public class AIConditionNode:JsonNodeBase
    {
        [ValueDropdown("@"+nameof(OdinDropdownHelper)+"."+nameof(OdinDropdownHelper.GetAIDecisionInterface)+"()")]
        public string Condition;


        public override void InitNode(Vector2 pos, string nodeName, int minInputPortsCount = 0, int minOutputPortsCount = 0)
        {
            base.InitNode(pos, nodeName, minInputPortsCount, minOutputPortsCount);
            SetName("Condition");
        }

        public override void AddDefaultPorts()
        {
            AddInputPort(EdgeMode.Override, false, false);
            AddOutputPort("True" , EdgeMode.Override, false, false);
            AddOutputPort("False" ,EdgeMode.Override, false, false);
        }
    }
}