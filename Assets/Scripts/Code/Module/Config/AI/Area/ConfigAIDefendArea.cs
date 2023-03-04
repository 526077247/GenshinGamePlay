using Nino.Serialization;

namespace TaoTie
{
    [NinoSerialize]
    public partial class ConfigAIDefendArea
    {
        [NinoMember(1)]
        public bool Enable = true;
        [NinoMember(2)]
        public float DefendRange; 
    }
}