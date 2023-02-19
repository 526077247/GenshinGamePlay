using Nino.Serialization;

namespace TaoTie
{
    [NinoSerialize]
    public partial class ConfigAISkillCastCondition
    {
        [NinoMember(1)]
        public int[] PoseIds;
        [NinoMember(2)]
        public float minTargetAngleXZ;
        [NinoMember(3)]
        public float maxTargetAngleXZ= 90;
        [NinoMember(4)]
        public float maxTargetAngleY = 90;
        [NinoMember(5)]
        public float minTargetAngleY;
        [NinoMember(6)]
        public float pickRangeMin;
        [NinoMember(7)]
        public float pickRangeMax;
        [NinoMember(8)]
        public float pickRangeYMax;
        [NinoMember(9)]
        public float pickRangeYMin;
        [NinoMember(10)]
        public float SkillAnchorRangeMin;
        [NinoMember(11)]
        public float SkillAnchorRangeMax;
        [NinoMember(12)]
        public float castRangeMin;
        [NinoMember(13)]
        public float castRangeMax;
    }
}