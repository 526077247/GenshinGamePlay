using DaGenGraph;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    public class StoryBranchClipItemNode: JsonNodeBase
    {
        [LabelText("选项显示内容")]
        public ConfigStoryText Text;
        public override void InitNode(Vector2 pos, string nodeName, int minInputPortsCount = 0, int minOutputPortsCount = 0)
        {
            canBeDeleted = true;
            base.InitNode(pos, nodeName, minInputPortsCount, minOutputPortsCount);
        }
        public override void AddDefaultPorts()
        {
            AddInputPort<StoryBranchPort>("上一步",EdgeMode.Override, false, EdgeType.Both, false);
            AddOutputPort("下一步", EdgeMode.Override, true, true);
        }
    }
}