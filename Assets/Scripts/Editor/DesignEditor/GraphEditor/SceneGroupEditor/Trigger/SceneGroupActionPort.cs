using DaGenGraph;
namespace TaoTie
{
    [PortGroup(3)]
    public class SceneGroupActionPort: Port
    {
        public override bool CanConnect(Port other, GraphBase graphBase, bool ignoreValueType = false)
        {
            if (base.CanConnect(other, graphBase, ignoreValueType))
            {
                var triggerType =
                    (graphBase as SceneGroupGraph)?.FindTriggerType(this.IsOutput() ? nodeId : other.nodeId);
                if (triggerType != null)
                {
                    // var otherNode = graphBase.FindNode(other.nodeId);
                    // var thisNode = graphBase.FindNode(nodeId);
                }
                return true;
            }

            return false;
        }
    }
}
