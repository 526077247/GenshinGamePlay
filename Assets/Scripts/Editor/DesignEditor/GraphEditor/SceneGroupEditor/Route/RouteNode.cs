using DaGenGraph;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [NodeViewType(typeof(RouteNodeView))]
    public class RouteNode: JsonNodeBase
    {
        [OnValueChanged(nameof(RefreshNode))][LabelText("批量编辑")]
        public bool ShowEditorPoints = false;
        
        [HideReferenceObjectPicker]
        public ConfigRoute Route = new ConfigRoute();

        [ShowIf(nameof(ShowEditorPoints))][PropertyOrder(1)][OnCollectionChanged(nameof(RefreshIndex))]
        public ConfigWaypoint[] Points;

        public override void InitNode(Vector2 pos, string nodeName, int minInputPortsCount = 0, int minOutputPortsCount = 0)
        {
            base.InitNode(pos, nodeName, minInputPortsCount, minOutputPortsCount);
            Route.LocalId = (int) (IdGenerater.Instance.GenerateId() % int.MaxValue);
        }
        private void RefreshIndex()
        {
            if(Points==null) return;
            for (int i = 0; i < Points.Length; i++)
            {
                if(Points[i]!=null) Points[i].Index = i;
            }
        }
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