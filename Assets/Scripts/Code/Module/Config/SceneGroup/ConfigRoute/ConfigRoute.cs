using ProtoBuf;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using TaoTie.Inspector;
#endif
using UnityEngine;

namespace TaoTie
{
    [ProtoContract]
    public partial class ConfigRoute
    {
#if UNITY_EDITOR
        [PropertyOrder(int.MinValue + 1)][LabelText("策划备注")]
        public string Remarks;
#endif
        [ProtoMember(1)]
        public int LocalId;
        [LabelText("路径类型")] [ProtoMember(2, IsRequired = true)] public RouteType Type = RouteType.OneWay;

        [LabelText("是否是前进")] [DisableInEditorMode] [ProtoMember(3, IsRequired = true)]
        public bool IsForward = true;

        [LabelText("旋转类型")] [ProtoMember(4)] public RotType RotType;

        [HideIf(nameof(RotType), RotType.ROT_NONE)] [ProtoMember(5, IsRequired = true)]
        public RotAngleType RotAngleType = RotAngleType.ROT_ANGLE_Y;

        [LabelText("判定抵达的范围")] [ProtoMember(6)] [MinValue(0.1f)] public float ArriveRange;
        
        [LabelText("判定角色靠近的范围")] [ProtoMember(8)] [MinValue(0.1f)] public float AvatarNearRange;
        [OnCollectionChanged(nameof(RefreshIndex))][Inspector.DrawIgnore]
        [ProtoMember(7)] public ConfigWaypoint[] Points;

        private void RefreshIndex()
        {
            if(Points==null) return;
            for (int i = 0; i < Points.Length; i++)
            {
                if(Points[i]!=null) Points[i].Index = i;
            }
        }
    }
}