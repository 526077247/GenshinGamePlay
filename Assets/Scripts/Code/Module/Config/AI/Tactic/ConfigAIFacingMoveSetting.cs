using System.Collections.Generic;
using Nino.Serialization;
using Sirenix.OdinInspector;

namespace TaoTie
{
    [LabelText("前后左右随机移动")]
    [NinoSerialize]
    public partial class ConfigAIFacingMoveSetting: ConfigAITacticBaseSetting
    {
        [NinoMember(10)]
        public ConfigAIFacingMoveData DefaultSetting;
        [NinoMember(11)]
        public Dictionary<int, ConfigAIFacingMoveData> Specification;
    }
}