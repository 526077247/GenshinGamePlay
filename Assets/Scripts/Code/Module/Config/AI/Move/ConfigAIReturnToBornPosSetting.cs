using Nino.Serialization;
using Sirenix.OdinInspector;

namespace TaoTie
{
    [LabelText("返回出生点")]
    [NinoSerialize]
    public class ConfigAIReturnToBornPosSetting: ConfigAITacticBaseSetting
    {
        [LabelText("启用")]
        [NinoMember(1)]
        public bool enable;
    }
}