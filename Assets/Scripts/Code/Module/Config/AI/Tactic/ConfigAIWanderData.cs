using Nino.Serialization;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [NinoSerialize]
    public partial class ConfigAIWanderData
    {
        [NinoMember(1)] 
        public MotionFlag SpeedLevel;
        [NinoMember(2)] 
        public float TurnSpeedOverride;
        [NinoMember(3)][LabelText("CD随机范围最大值(ms)")]
        public int CdMax;
        [NinoMember(4)][LabelText("CD随机范围最小值(ms)")]
        public int CdMin;
        [NinoMember(5)][LabelText("最大漫游半径")]
        public float DistanceFromBorn = 5;
        [NinoMember(6)][LabelText("每次随机移动最小距离")][Min(0)]
        public float DistanceFromCurrentMin = 0;
        [NinoMember(7)][LabelText("每次随机移动最大距离")][Min(0)]
        public float DistanceFromCurrentMax = 1;
        [NinoMember(8)] 
        public AIBasicMoveType MoveType;
    }
}