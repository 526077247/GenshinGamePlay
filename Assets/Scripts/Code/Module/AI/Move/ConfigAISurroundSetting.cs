using Nino.Serialization;
using Sirenix.OdinInspector;

namespace TaoTie
{
    [LabelText("环绕对峙")]
    [NinoSerialize]
    public partial class ConfigAISurroundSetting: ConfigAITacticBaseSetting
    {
        [LabelText("启用")]
        [NinoMember(1)]
        public bool enable;
    }
}