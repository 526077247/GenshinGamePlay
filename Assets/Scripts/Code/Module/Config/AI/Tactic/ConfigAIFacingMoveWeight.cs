using Nino.Serialization;

namespace TaoTie
{
    [NinoSerialize]
    public partial class ConfigAIFacingMoveWeight
    {
        [NinoMember(1)]
        public float Stop;
        [NinoMember(2)]
        public float Forward;
        [NinoMember(3)]
        public float Back;
        [NinoMember(4)]
        public float Left;
        [NinoMember(5)]
        public float Right;

    }
}