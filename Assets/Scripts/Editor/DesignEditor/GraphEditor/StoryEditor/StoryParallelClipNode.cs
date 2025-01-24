using DaGenGraph;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    public class StoryParallelClipNode: JsonNodeBase
    {
        [LabelText("等待所有子项执行完成")]
        public bool WaitAll;
        public override void InitNode(Vector2 pos, string nodeName, int minInputPortsCount = 0, int minOutputPortsCount = 0)
        {
            canBeDeleted = true;
            base.InitNode(pos, nodeName, minInputPortsCount, minOutputPortsCount);
        }
        public override void AddDefaultPorts()
        {
            AddInputPort("上一步",EdgeMode.Override, false, false);
            AddOutputPort("下一步", EdgeMode.Override, true, true);
            AddOutputPort("并行项", EdgeMode.Multiple, true, true);
        }
    }
}