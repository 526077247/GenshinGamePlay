using Nino.Serialization;

namespace TaoTie
{
    [NinoSerialize]
    public partial class ConfigAIFacingMoveData
    {
        [NinoMember(1)]
        public AIMoveSpeedLevel SpeedLevel;
        [NinoMember(2)]
        public float RangeMin;
        [NinoMember(3)]
        public float RangeMax;
        [NinoMember(4)]
        public float RestTimeMin;
        [NinoMember(5)]
        public float RestTimeMax;
        [NinoMember(6)]
        public float FacingMoveTurnInterval;
        [NinoMember(7)]
        public float FacingMoveMinAvoidanceVelocity;
        [NinoMember(8)]
        public float ObstacleDetectRange;
        [NinoMember(9)]
        public ConfigAIFacingMoveWeight FacingMoveWeight; 
    }
}