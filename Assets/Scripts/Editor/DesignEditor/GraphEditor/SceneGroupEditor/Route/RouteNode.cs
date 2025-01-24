using DaGenGraph;
using Sirenix.OdinInspector;

namespace TaoTie
{
    [NodeViewType(typeof(RouteNodeView))]
    public class RouteNode: JsonNodeBase
    {
        [OnValueChanged(nameof(RefreshNode))][LabelText("批量编辑")]
        public bool ShowEditorPoints = false;
        
        [HideReferenceObjectPicker]
        public ConfigRoute Route = new ConfigRoute();

        [ShowIf(nameof(ShowEditorPoints))][PropertyOrder(1)]
        public ConfigWaypoint[] Points;

        public void RefreshNode()
        {
            if (!ShowEditorPoints)
            {
                if(outputPorts.Count <= 0)
                    AddOutputPort<WaypointPort>("路径", EdgeMode.Override, false, EdgeType.Both, false);
            }
            else
            {
                if (outputPorts.Count > 0)
                    DeletePort(outputPorts[0]);
            }
        }
        public override void AddDefaultPorts()
        {
            AddInputPort<SetRouteIdPort>("引用",EdgeMode.Multiple, false, EdgeType.Both, false);
            AddOutputPort<WaypointPort>("路径", EdgeMode.Override, false, EdgeType.Both, false);
        }
    }
}