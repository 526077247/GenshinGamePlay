using DaGenGraph;
using Sirenix.OdinInspector;

namespace TaoTie
{
    public class SceneGroupTriggerNode: JsonNodeBase
    {
        [LabelText("监听")]
        public ConfigSceneGroupTrigger Trigger;
        public override void AddDefaultPorts()
        {
            AddOutputPort<SceneGroupActionPort>("当触发后", EdgeMode.Multiple, false, EdgeType.Both, false);
        }
    }
}