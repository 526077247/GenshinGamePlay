using Nino.Serialization;

namespace TaoTie
{
    [NinoSerialize]
    public partial class ConfigAIWanderData
    {
        [NinoMember(1)] 
        public int SpeedLevel;
        [NinoMember(2)] 
        public float TurnSpeedOverride;
        [NinoMember(3)] 
        public int CdMax;
        [NinoMember(4)] 
        public int CdMin;
        [NinoMember(5)] 
        public float DistanceFromBorn;
        [NinoMember(6)] 
        public float DistanceFromCurrentMin;
        [NinoMember(7)] 
        public float DistanceFromCurrentMax;
        [NinoMember(8)] 
        public AIBasicMoveType MoveType;
    }
}