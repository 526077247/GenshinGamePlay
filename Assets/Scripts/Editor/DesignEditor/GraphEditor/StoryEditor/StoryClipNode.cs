using UnityEngine;
using TaoTie.Inspector;

namespace TaoTie
{
    [NodeViewType(typeof(StoryClipNodeView))]
    public class StoryClipNode: JsonNodeBase
    {
        public ConfigStoryClip Data;

        public override void InitNode(Vector2 pos, string nodeName, int minInputPortsCount = 0, int minOutputPortsCount = 0)
        {
            canBeDeleted = true;
            base.InitNode(pos, nodeName, minInputPortsCount, minOutputPortsCount);
        }

        public override void AddDefaultPorts()
        {
            AddInputPort("上一步",EdgeMode.Override, false, false);
            AddOutputPort("下一步", EdgeMode.Override, true, true);
        }
    }
}