using Nino.Serialization;

namespace TaoTie
{
    [NinoSerialize]
    public partial class ConfigAIFacingMoveWeight
    {
        [NinoMember(1)]
        public float stop;
        [NinoMember(2)]
        public float forward;
        [NinoMember(3)]
        public float back;
        [NinoMember(4)]
        public float left;
        [NinoMember(5)]
        public float right;

    }
}