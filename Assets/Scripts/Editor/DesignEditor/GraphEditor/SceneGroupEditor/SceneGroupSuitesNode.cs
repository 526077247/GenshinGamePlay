using DaGenGraph;
using TaoTie.LitJson.Extensions;
using Sirenix.OdinInspector;

namespace TaoTie
{
    public class SceneGroupSuitesNode: JsonNodeBase
    {
        [JsonIgnore]
        public bool RandSuite => SceneGroupGraphWindow.Instance?.m_Graph?.RandSuite??false;
        
        [LabelText("策划备注")][PropertyOrder(int.MinValue+1)]
        public string Remarks;
        public int Id;
        [ValueDropdown("@" + nameof(OdinDropdownHelper) + "." + nameof(OdinDropdownHelper.GetSceneGroupActorIds) + "()",AppendNextDrawer = true)]
        public int[] Actors;
        [ValueDropdown("@" + nameof(OdinDropdownHelper) + "." + nameof(OdinDropdownHelper.GetSceneGroupZoneIds) + "()",AppendNextDrawer = true)]
        public int[] Zones;
        [ShowIf(nameof(RandSuite))][LabelText("随机权值")]
        public int RandWeight;
        public override void AddDefaultPorts()
        {
            Id = (int) (IdGenerater.Instance.GenerateId() % int.MaxValue);
            AddOutputPort<SceneGroupTriggerPort>("监听事件", EdgeMode.Multiple, false, EdgeType.Both, false);
        }
    }
}