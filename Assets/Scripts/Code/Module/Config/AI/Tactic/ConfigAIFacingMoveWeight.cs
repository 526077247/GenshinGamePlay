using Nino.Serialization;

namespace TaoTie
{
    [NinoSerialize]
    public partial class ConfigAIFacingMoveWeight
    {
        [NinoMember(1)]
        private float stop;
        [NinoMember(2)]
        private float forward;
        [NinoMember(3)]
        private float back;
        [NinoMember(4)]
        private float left;
        [NinoMember(5)]
        private float right;

    }
}