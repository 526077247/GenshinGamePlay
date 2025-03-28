using System.Collections.Generic;
using Nino.Core;
using Sirenix.OdinInspector;

namespace TaoTie
{
    [LabelText("前后左右随机移动")]
    [NinoType(false)]
    public partial class ConfigAIFacingMoveSetting: ConfigAITacticBaseSetting
    {
        [NinoMember(10)][NotNull]
        public ConfigAIFacingMoveData DefaultSetting;
        [NinoMember(11)]
        public Dictionary<int, ConfigAIFacingMoveData> Specification = new Dictionary<int, ConfigAIFacingMoveData>();
    }
}