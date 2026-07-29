using ProtoBuf;
using Sirenix.OdinInspector;

namespace TaoTie
{
    [ProtoContract]
    public partial class ConfigEnvironments
    {
        [ProtoMember(1)]
        public ConfigEnvironment DefaultEnvironment;
        [ProtoMember(2)]
        public ConfigEnvironment[] Environments;
        [ProtoMember(3, IsRequired = true)] [HideReferenceObjectPicker]
        public ConfigBlender DefaultBlend = new ConfigBlender();
    }
}