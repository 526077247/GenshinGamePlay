using ProtoBuf;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using TaoTie.Inspector;
#endif

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