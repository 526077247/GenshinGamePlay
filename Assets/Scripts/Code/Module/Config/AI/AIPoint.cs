using Nino.Serialization;

namespace TaoTie
{
    [NinoSerialize]
    public partial class AIPoint
    {
        [NinoMember(1)]
        public float X;
        [NinoMember(2)]
        public float Y;
    }
}