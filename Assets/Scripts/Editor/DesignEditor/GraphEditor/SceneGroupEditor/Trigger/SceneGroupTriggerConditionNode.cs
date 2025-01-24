using DaGenGraph;
using Sirenix.OdinInspector;

namespace TaoTie
{
    [NodeViewType(typeof(SceneGroupTriggerConditionNodeView))]
    public class SceneGroupTriggerConditionNode: JsonNodeBase
    {
        [LabelText("判断")]
        public ConfigSceneGroupCondition Condition;
        public override void AddDefaultPorts()
        {
            AddInputPort<SceneGroupActionPort>("执行", EdgeMode.Multiple, false, EdgeType.Both, false);
            AddOutputPort<SceneGroupActionPort>("满足条件后", EdgeMode.Multiple, false, EdgeType.Both, false);
            AddOutputPort<SceneGroupActionPort>("不满足则", EdgeMode.Multiple, false, EdgeType.Both, false);
        }
    }
}