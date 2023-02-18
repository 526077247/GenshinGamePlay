using System.Collections.Generic;
using Nino.Serialization;
using Sirenix.OdinInspector;

namespace TaoTie
{
    [LabelText("前后左右随机移动")]
    [NinoSerialize]
    public partial class ConfigAIFacingMoveSetting: ConfigAITacticBaseSetting
    {

        [LabelText("启用")]
        [NinoMember(1)]
        public bool enable;
        public ConfigAIFacingMoveData defaultSetting;
        public Dictionary<int, ConfigAIFacingMoveData> specification;
    }
}