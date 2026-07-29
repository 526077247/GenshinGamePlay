using ProtoBuf;

namespace TaoTie
{
    [ProtoContract]
    public partial class AIPoint
    {
        [ProtoMember(1)]
        public float X;
        [ProtoMember(2)]
        public float Y;
    }
}