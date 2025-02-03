using DaGenGraph;
using Sirenix.OdinInspector;

namespace TaoTie
{
    [NodeViewType(typeof(SceneGroupTriggerConditionNodeView))]
    public class SceneGroupTriggerLogicConditionNode: JsonNodeBase
    {
        [LabelText("判断项")]
        public ConfigSceneGroupCondition[] Condition;
        [LabelText("逻辑或(True)与(False)")]
        public bool Mode;
        public override void AddDefaultPorts()
        {
            AddInputPort<SceneGroupActionPort>("执行", EdgeMode.Multiple, false, EdgeType.Both, false);
            AddOutputPort<SceneGroupActionPort>("满足条件后", EdgeMode.Multiple, false, EdgeType.Both, false);
            AddOutputPort<SceneGroupActionPort>("不满足则", EdgeMode.Multiple, false, EdgeType.Both, false);
        }
    }
}