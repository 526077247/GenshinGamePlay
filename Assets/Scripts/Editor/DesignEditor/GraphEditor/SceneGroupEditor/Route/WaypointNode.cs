using DaGenGraph;
using Sirenix.OdinInspector;

namespace TaoTie
{
    public class WaypointNode: JsonNodeBase
    {
        [HideReferenceObjectPicker]
        public ConfigWaypoint Point = new ConfigWaypoint();
        public override void AddDefaultPorts()
        {
            AddInputPort<WaypointPort>("上一路径点",EdgeMode.Override, false, EdgeType.Both, false);
            AddOutputPort<WaypointPort>("下一路径点", EdgeMode.Override, false, EdgeType.Both, false);
        }
    }
}