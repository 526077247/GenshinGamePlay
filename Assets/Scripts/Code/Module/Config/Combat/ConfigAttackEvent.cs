using ProtoBuf;

namespace TaoTie
{
    [ProtoContract]
    public partial class ConfigAttackEvent
    {
        [NotNull][ProtoMember(1)]
        public ConfigBaseAttackPattern AttackPattern;
        [NotNull][ProtoMember(2, IsRequired = true)]
        public ConfigAttackInfo AttackInfo = new ConfigAttackInfo();
    }
}