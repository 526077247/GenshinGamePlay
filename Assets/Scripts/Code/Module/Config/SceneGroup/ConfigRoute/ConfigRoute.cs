using Nino.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [NinoType(false)]
    public partial class ConfigRoute
    {
#if UNITY_EDITOR
        [PropertyOrder(int.MinValue + 1)][LabelText("策划备注")]
        public string Remarks;
#endif
        [NinoMember(1)] public int LocalId;
        [LabelText("路径类型")] [NinoMember(2)] public RouteType Type = RouteType.OneWay;

        [LabelText("是否是前进")] [DisableInEditorMode] [NinoMember(3)]
        public bool IsForward = true;

        [LabelText("旋转类型")] [NinoMember(4)] public RotType RotType;

        [HideIf(nameof(RotType), RotType.ROT_NONE)] [NinoMember(5)]
        public RotAngleType RotAngleType = RotAngleType.ROT_ANGLE_Y;

        [LabelText("判定抵达的范围")] [NinoMember(6)] [MinValue(0.1f)] public float ArriveRange;
        
        [LabelText("判定角色靠近的范围")] [NinoMember(8)] [MinValue(0.1f)] public float AvatarNearRange;
        [OnCollectionChanged(nameof(RefreshIndex))]
        [NinoMember(7)] public ConfigWaypoint[] Points;

        private void RefreshIndex()
        {
            if(Points==null) return;
            for (int i = 0; i < Points.Length; i++)
            {
                Points[i].Index = i;
            }
        }
    }
}