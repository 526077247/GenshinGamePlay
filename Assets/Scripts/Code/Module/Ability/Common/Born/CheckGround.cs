using Nino.Serialization;

namespace TaoTie
{
    [NinoSerialize]
    public class CheckGround
    {
        [NinoMember(1)]
        public bool enable;
        [NinoMember(2)]
        public float raycastUpHeight;
        [NinoMember(3)]
        public float raycastDownHeight;
        [NinoMember(4)]
        public bool stickToGroundIfValid;
        [NinoMember(5)]
        public bool dontCreateIfInvalid;
    }
}