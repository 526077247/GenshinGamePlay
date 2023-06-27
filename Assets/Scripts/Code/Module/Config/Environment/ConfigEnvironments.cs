using Nino.Serialization;
using Sirenix.OdinInspector;

namespace TaoTie
{
    [NinoSerialize()]
    public partial class ConfigEnvironments
    {
        [NinoMember(1)]
        public ConfigEnvironment DefaultEnvironment;
        [NinoMember(2)]
        public ConfigEnvironment[] Environments;
        [NinoMember(3)] [HideReferenceObjectPicker]
        public ConfigBlender DefaultBlend = new ConfigBlender();
    }
}