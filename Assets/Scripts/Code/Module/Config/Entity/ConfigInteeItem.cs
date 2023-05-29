using Nino.Serialization;
using Sirenix.OdinInspector;

namespace TaoTie
{
    [NinoSerialize]
    public partial class ConfigInteeItem
    {
        [NinoMember(1)]
        public int LocalId;
        [NinoMember(2)]
        public string I18NKey;
        [NinoMember(3)]
        public string[] I18NParams;
        [NinoMember(4)] [LabelText("默认启用")]
        public bool DefaultEnable = true;
    }
}