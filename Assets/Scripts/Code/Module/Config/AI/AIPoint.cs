using Nino.Serialization;

namespace TaoTie
{
    [NinoSerialize]
    public class AIPoint
    {
        [NinoMember(1)]
        public float x;
        [NinoMember(2)]
        public float y;
    }
}