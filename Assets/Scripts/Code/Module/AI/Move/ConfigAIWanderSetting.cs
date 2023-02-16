using Nino.Serialization;
using Sirenix.OdinInspector;

namespace TaoTie
{
    [LabelText("漫游")]
    [NinoSerialize]
    public partial class ConfigAIWanderSetting: ConfigAITacticBaseSetting
    {
        [LabelText("启用")]
        [NinoMember(1)]
        public bool enable;
    }
}