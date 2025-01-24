using DaGenGraph;
using Sirenix.OdinInspector;

namespace TaoTie
{
    [NodeViewType(typeof(SceneGroupTriggerActionNodeView))]
    public class SceneGroupTriggerActionNode: JsonNodeBase
    {
        [LabelText("事件")]
        public ConfigSceneGroupAction Action;
        public override void AddDefaultPorts()
        {
            AddInputPort<SceneGroupActionPort>("执行", EdgeMode.Multiple, false, EdgeType.Both, false);
        }
    }
}