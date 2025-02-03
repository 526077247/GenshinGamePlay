using DaGenGraph;
using DaGenGraph.Editor;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    public class AIConditionNode:JsonNodeBase
    {
        [ValueDropdown("@"+nameof(OdinDropdownHelper)+"."+nameof(OdinDropdownHelper.GetAIDecisionInterface)+"()")]
        [LabelText("判断")]
        public string Condition;


        public override void InitNode(Vector2 pos, string nodeName, int minInputPortsCount = 0, int minOutputPortsCount = 0)
        {
            base.InitNode(pos, nodeName, minInputPortsCount, minOutputPortsCount);
            SetName("Condition");
        }

        public override void AddDefaultPorts()
        {
            AddInputPort("输入", EdgeMode.Override, false, false);
            AddOutputPort("True" , EdgeMode.Override, false, false);
            AddOutputPort("False" ,EdgeMode.Override, false, false);
        }
    }
}