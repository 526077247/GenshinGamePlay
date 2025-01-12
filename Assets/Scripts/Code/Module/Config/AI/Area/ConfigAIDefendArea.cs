using Nino.Core;

namespace TaoTie
{
    [NinoType(false)]
    public partial class ConfigAIDefendArea
    {
        [NinoMember(1)]
        public bool Enable = true;
        [NinoMember(2)]
        public float DefendRange; 
    }
}