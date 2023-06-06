using Nino.Serialization;
using UnityEngine;

namespace TaoTie
{
    [NinoSerialize]
    public partial class ConfigAISkillCastCondition
    {
        [NinoMember(1)]
        public int[] PoseIds;
        [NinoMember(2)][Min(0)]
        public float MinTargetAngleXZ;
        [NinoMember(3)][Min(0)]
        public float MaxTargetAngleXZ= 90;
        [NinoMember(4)][Min(0)]
        public float MinTargetAngleY;
        [NinoMember(5)][Min(0)]
        public float MaxTargetAngleY = 90;
        [NinoMember(6)][Min(0)]
        public float PickRangeMin;
        [NinoMember(7)][Min(0)]
        public float PickRangeMax = 1;
        [NinoMember(8)][Min(0)]
        public float PickRangeYMin;
        [NinoMember(9)][Min(0)]
        public float PickRangeYMax = 1;
        [NinoMember(10)][Min(0)]
        public float SkillAnchorRangeMin;
        [NinoMember(11)][Min(0)]
        public float SkillAnchorRangeMax = 1;
        [NinoMember(12)][Min(0)]
        public float CastRangeMin;
        [NinoMember(13)][Min(0)]
        public float CastRangeMax = 1;
    }
}