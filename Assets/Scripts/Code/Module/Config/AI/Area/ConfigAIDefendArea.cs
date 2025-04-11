using Nino.Core;
using Sirenix.OdinInspector;

namespace TaoTie
{
    [NinoType(false)]
    public partial class ConfigAIDefendArea
    {
        [NinoMember(1)]
        public bool Enable = true;
        [NinoMember(2)][LabelText("防守距边界范围")]
        public float DefendRange; 
    }
}