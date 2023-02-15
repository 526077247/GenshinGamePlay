using Nino.Serialization;

namespace TaoTie
{
    [NinoSerialize]
    public partial class ConfigAISkillCastCondition
    {
        [NinoMember(1)]
        public int[] PoseIds;
        public float minTargetAngleXZ;
        public float maxTargetAngleXZ;
        public float maxTargetAngleY;
        public float minTargetAngleY;
        public float pickRangeMin;
        public float pickRangeMax;
        public float pickRangeYMax;
        public float pickRangeYMin;
        public float SkillAnchorRangeMin;
        public float SkillAnchorRangeMax;
        public float castRangeMin;
        public float castRangeMax;
    }
}