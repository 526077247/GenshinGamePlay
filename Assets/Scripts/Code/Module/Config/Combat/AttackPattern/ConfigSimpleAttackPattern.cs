using ProtoBuf;

namespace TaoTie
{
    [ProtoContract]
    [ProtoInclude(100, typeof(ConfigAttackBox))]
    [ProtoInclude(101, typeof(ConfigAttackSphere))]
    public abstract class ConfigSimpleAttackPattern: ConfigBaseAttackPattern
    {
        [NotNull] [ProtoMember(3, IsRequired = true)]
        public ConfigBornType Born = new ConfigBornByTarget();
    }
}