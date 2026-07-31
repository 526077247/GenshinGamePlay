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
    public partial class ConfigAIWanderData
    {
        [ProtoMember(1)] 
        public MotionFlag SpeedLevel;
        [ProtoMember(2)] 
        public float TurnSpeedOverride;
        [ProtoMember(3)][LabelText("CD随机范围最大值(ms)")]
        public int CdMax;
        [ProtoMember(4)][LabelText("CD随机范围最小值(ms)")]
        public int CdMin;
        [ProtoMember(5, IsRequired = true)][LabelText("最大漫游半径")]
        public float DistanceFromBorn = 5;
        [ProtoMember(6, IsRequired = true)][LabelText("每次随机移动最小距离")][MinValue(0)]
        public float DistanceFromCurrentMin = 0;
        [ProtoMember(7, IsRequired = true)][LabelText("每次随机移动最大距离")][MinValue(0)]
        public float DistanceFromCurrentMax = 1;
        [ProtoMember(8)] 
        public AIBasicMoveType MoveType;
    }
}