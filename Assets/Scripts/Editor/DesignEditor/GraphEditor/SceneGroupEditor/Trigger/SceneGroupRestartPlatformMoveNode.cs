using DaGenGraph;

namespace TaoTie
{
    public class SceneGroupRestartPlatformMoveNode: JsonNodeBase
    {
        public int ActorId;
        public override void AddDefaultPorts()
        {
            AddInputPort<SceneGroupActionPort>("执行", EdgeMode.Multiple, false, EdgeType.Both, false);
            AddOutputPort<SetRouteIdPort>("寻路路径", EdgeMode.Override, false, EdgeType.Both, false);
        }
    }
}