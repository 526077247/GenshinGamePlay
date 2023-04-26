using Nino.Serialization;

namespace TaoTie
{
    [NinoSerialize]
    public partial class ConfigAISkillCastCondition
    {
        [NinoMember(1)]
        public int[] PoseIds;
        [NinoMember(2)]
        public float MinTargetAngleXZ;
        [NinoMember(3)]
        public float MaxTargetAngleXZ= 90;
        [NinoMember(4)]
        public float MaxTargetAngleY = 90;
        [NinoMember(5)]
        public float MinTargetAngleY;
        [NinoMember(6)]
        public float PickRangeMin;
        [NinoMember(7)]
        public float PickRangeMax;
        [NinoMember(8)]
        public float PickRangeYMax;
        [NinoMember(9)]
        public float PickRangeYMin;
        [NinoMember(10)]
        public float SkillAnchorRangeMin;
        [NinoMember(11)]
        public float SkillAnchorRangeMax;
        [NinoMember(12)]
        public float CastRangeMin;
        [NinoMember(13)]
        public float CastRangeMax;
    }
}