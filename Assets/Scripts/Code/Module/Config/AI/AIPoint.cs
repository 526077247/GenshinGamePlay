using Nino.Serialization;

namespace TaoTie
{
    [NinoSerialize]
    public partial class AIPoint
    {
        [NinoMember(1)]
        public float x;
        [NinoMember(2)]
        public float y;
    }
}