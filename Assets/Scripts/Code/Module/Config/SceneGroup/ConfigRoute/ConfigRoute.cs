using Nino.Serialization;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [NinoSerialize]
    public partial class ConfigRoute
    {
#if UNITY_EDITOR
        [PropertyOrder(int.MinValue + 1)] [SerializeField] [LabelText("策划备注")]
        private string Remarks;
#endif
        [NinoMember(1)] public int LocalId;
        [LabelText("路径类型")] [NinoMember(2)] public RouteType Type = RouteType.OneWay;

        [LabelText("是否是前进")] [DisableInEditorMode] [NinoMember(3)]
        public bool IsForward = true;

        [LabelText("旋转类型")] [NinoMember(4)] public RotType RotType;

        [HideIf(nameof(RotType), RotType.ROT_NONE)] [NinoMember(5)]
        public RotAngleType RotAngleType = RotAngleType.ROT_ANGLE_Y;

        [LabelText("判定抵达的范围")] [NinoMember(6)] public float ArriveRange;
        [NinoMember(7)] public ConfigWaypoint[] Points;
    }
}