using ProtoBuf;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [ProtoContract]
    public partial class ConfigAIFacingMoveData
    {
        [ProtoMember(1)]
        public MotionFlag SpeedLevel;
        [ProtoMember(2)][MinValue(0.1f)]
        public float RangeMin;
        [ProtoMember(3)][MinValue(0.1f)]
        public float RangeMax;
        [ProtoMember(4)][LabelText("随机重置时间min（ms）")]
        public int RestTimeMin;
        [ProtoMember(5)][LabelText("随机重置时间max（ms）")]
        public int RestTimeMax;
        [ProtoMember(6)]
        public int FacingMoveTurnInterval;
        [ProtoMember(7)]
        public float FacingMoveMinAvoidanceVelocity;
        [ProtoMember(8)][LabelText("检测靠近障碍的距离")]
        public float ObstacleDetectRange;
        [ProtoMember(9)]
        public ConfigAIFacingMoveWeight FacingMoveWeight; 
    }
}