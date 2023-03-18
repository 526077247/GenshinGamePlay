using Nino.Serialization;

namespace TaoTie
{
    [NinoSerialize]
    public partial class ConfigAIFacingMoveData
    {
        [NinoMember(1)]
        public int speedLevel;
        [NinoMember(2)]
        public float rangeMin;
        [NinoMember(3)]
        public float rangeMax;
        [NinoMember(4)]
        public float restTimeMin;
        [NinoMember(5)]
        public float restTimeMax;
        [NinoMember(6)]
        public float facingMoveTurnInterval;
        [NinoMember(7)]
        public float facingMoveMinAvoidanceVelecity;
        [NinoMember(8)]
        public float obstacleDetectRange;
        [NinoMember(9)]
        public ConfigAIFacingMoveWeight facingMoveWeight; 
    }
}