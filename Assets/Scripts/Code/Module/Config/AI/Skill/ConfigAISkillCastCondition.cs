using ProtoBuf;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [ProtoContract]
    public partial class ConfigAISkillCastCondition
    {
        [ProtoMember(1)][LabelText("*PoseIds")][Tooltip("处于这些Pose中时有效, 为null表示全有效")]
        public int[] PoseIds;
        [ProtoMember(2)][MinValue(0)][LabelText("与视线水平角度min")][BoxGroup("目标选取")]
        public float MinTargetAngleXZ;
        [ProtoMember(3, IsRequired = true)][MinValue(0)][LabelText("与视线水平角度max")][BoxGroup("目标选取")]
        public float MaxTargetAngleXZ= 90;
        [ProtoMember(4)][MinValue(0)][LabelText("与视线y角度min")][BoxGroup("目标选取")]
        public float MinTargetAngleY;
        [ProtoMember(5, IsRequired = true)][MinValue(0)][LabelText("与视线y角度max")][BoxGroup("目标选取")]
        public float MaxTargetAngleY = 90;
        [ProtoMember(6)][MinValue(0)][LabelText("距离min")][BoxGroup("目标选取")]
        public float PickRangeMin;
        [ProtoMember(7, IsRequired = true)][MinValue(0)][LabelText("距离max")][BoxGroup("目标选取")]
        public float PickRangeMax = 1;
        [ProtoMember(8, IsRequired = true)][LabelText("高度差min")][BoxGroup("目标选取")]
        public float PickRangeYMin = -1;
        [ProtoMember(9, IsRequired = true)][LabelText("高度差max")][BoxGroup("目标选取")]
        public float PickRangeYMax = 1;
        [ProtoMember(10)][MinValue(0)][LabelText("持续施法距离min")]
        public float SkillAnchorRangeMin;
        [ProtoMember(11, IsRequired = true)][MinValue(0)][LabelText("持续施法距离max")]
        public float SkillAnchorRangeMax = 1;
        [ProtoMember(12)][MinValue(0)][LabelText("施法距离min")]
        public float CastRangeMin;
        [ProtoMember(13, IsRequired = true)][MinValue(0)][LabelText("施法距离max")]
        public float CastRangeMax = 1;
    }
}