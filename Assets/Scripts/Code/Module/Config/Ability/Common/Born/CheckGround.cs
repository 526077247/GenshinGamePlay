using Nino.Serialization;

namespace TaoTie
{
    [NinoSerialize]
    public class CheckGround
    {
        [NinoMember(1)]
        public bool Enable;
        [NinoMember(2)]
        public float RaycastUpHeight;
        [NinoMember(3)]
        public float RaycastDownHeight;
        [NinoMember(4)]
        public bool StickToGroundIfValid;
        [NinoMember(5)]
        public bool DontCreateIfInvalid;
    }
}