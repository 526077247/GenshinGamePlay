using Nino.Serialization;
using Sirenix.OdinInspector;

namespace TaoTie
{
    [LabelText("冲到面前")]
    [NinoSerialize]
    public class ConfigAIMeleeChargeSetting: ConfigAITacticBaseSetting
    {
        [LabelText("启用")]
        [NinoMember(1)]
        public bool enable;
    }
}