using ProtoBuf;

namespace TaoTie
{
    [ProtoContract]
    public partial class ConfigAIFacingMoveWeight
    {
        [ProtoMember(1)]
        public float Stop;
        [ProtoMember(2)]
        public float Forward;
        [ProtoMember(3)]
        public float Back;
        [ProtoMember(4)]
        public float Left;
        [ProtoMember(5)]
        public float Right;

    }
}