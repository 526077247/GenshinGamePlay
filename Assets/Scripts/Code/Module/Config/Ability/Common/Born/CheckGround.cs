using ProtoBuf;

namespace TaoTie
{
    [ProtoContract]
    public class CheckGround
    {
        [ProtoMember(1)]
        public bool Enable;
        [ProtoMember(2)]
        public float RaycastUpHeight;
        [ProtoMember(3)]
        public float RaycastDownHeight;
        [ProtoMember(4)]
        public bool StickToGroundIfValid;
        [ProtoMember(5)]
        public bool DontCreateIfInvalid;
    }
}