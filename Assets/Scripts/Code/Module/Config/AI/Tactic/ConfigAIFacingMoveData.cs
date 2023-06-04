using Nino.Serialization;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [NinoSerialize]
    public partial class ConfigAIFacingMoveData
    {
        [NinoMember(1)]
        public MotionFlag SpeedLevel;
        [NinoMember(2)][Min(0.1f)]
        public float RangeMin;
        [NinoMember(3)][Min(0.1f)]
        public float RangeMax;
        [NinoMember(4)][LabelText("随机重置时间min（ms）")]
        public long RestTimeMin;
        [NinoMember(5)][LabelText("随机重置时间max（ms）")]
        public long RestTimeMax;
        [NinoMember(6)]
        public long FacingMoveTurnInterval;
        [NinoMember(7)]
        public float FacingMoveMinAvoidanceVelocity;
        [NinoMember(8)][LabelText("检测靠近障碍的距离")]
        public float ObstacleDetectRange;
        [NinoMember(9)]
        public ConfigAIFacingMoveWeight FacingMoveWeight; 
    }
}