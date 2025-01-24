using DaGenGraph;
using Sirenix.OdinInspector;

namespace TaoTie
{
    public class SceneGroupTriggerNode: JsonNodeBase
    {
        [LabelText("监听项")]
        public ConfigSceneGroupTrigger Trigger;
        public override void AddDefaultPorts()
        {
            AddInputPort<SceneGroupTriggerPort>("监听", EdgeMode.Multiple, false, EdgeType.Both, false);
            AddOutputPort<SceneGroupActionPort>("当触发后", EdgeMode.Multiple, false, EdgeType.Both, false);
        }
    }
}