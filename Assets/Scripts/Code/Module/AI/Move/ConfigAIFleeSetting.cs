using Nino.Serialization;
using Sirenix.OdinInspector;

namespace TaoTie
{
    [LabelText("逃跑")]
    [NinoSerialize]
    public partial class ConfigAIFleeSetting: ConfigAITacticBaseSetting
    {
        [LabelText("启用")]
        [NinoMember(1)]
        public bool enable;
    }
}