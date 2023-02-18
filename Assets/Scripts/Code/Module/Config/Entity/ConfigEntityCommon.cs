using Nino.Serialization;

namespace TaoTie
{
    [NinoSerialize]
    public partial class ConfigEntityCommon
    {
        [NinoMember(1)]
        public float Height;
        [NinoMember(2)]
        public float ModelHeight;
        [NinoMember(3)]
        public float Scale;
        
    }
}