using Nino.Serialization;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [NinoSerialize]
    public partial class ConfigRoute
    {
#if UNITY_EDITOR
        [PropertyOrder(int.MinValue + 1)] [LabelText("策划备注")]
        public string remarks;
#endif
        [NinoMember(1)] public int localId;
        [LabelText("路径类型")] [NinoMember(2)] public RouteType type = RouteType.OneWay;

        [LabelText("是否是前进")] [DisableInEditorMode] [NinoMember(3)]
        public bool isForward = true;

        [LabelText("旋转类型")] [NinoMember(4)] public RotType rotType;

        [HideIf("rotType", RotType.ROT_NONE)] [NinoMember(5)]
        public RotAngleType rotAngleType = RotAngleType.ROT_ANGLE_Y;

        [LabelText("判定抵达的范围")] [NinoMember(6)] public float arriveRange;
        [NinoMember(7)] public ConfigWaypoint[] points;
    }
}