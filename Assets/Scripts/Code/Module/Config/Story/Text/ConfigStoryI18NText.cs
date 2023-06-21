using Nino.Serialization;
using Sirenix.OdinInspector;

namespace TaoTie
{
    [NinoSerialize]
    public class ConfigStoryI18NText: ConfigStoryText
    {
        [NinoMember(1)][LabelText("多语言表Key")]
        public string I18NKey;

        public override string GetShowText()
        {
            return I18NManager.Instance.I18NGetText(I18NKey);
        }
    }
}