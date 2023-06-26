using Nino.Serialization;

namespace TaoTie
{
    [NinoSerialize()]
    public partial class EnvironmentConfig
    {
        [NinoMember(1)]
        public int Id;
        [NinoMember(2)]
        public ConfigBlender Enter;
        [NinoMember(3)]
        public ConfigBlender Leave;
    }
}