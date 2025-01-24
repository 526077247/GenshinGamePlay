using DaGenGraph;

namespace TaoTie
{
    public class SceneGroupSuitesNode: JsonNodeBase
    {
        public int[] Actors;

        public int[] Zones;

        public override void AddDefaultPorts()
        {
            AddOutputPort<SceneGroupTriggerPort>("监听事件", EdgeMode.Multiple, false, EdgeType.Both, false);
        }
    }
}