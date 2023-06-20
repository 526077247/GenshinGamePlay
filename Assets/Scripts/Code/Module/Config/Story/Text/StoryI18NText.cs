using Nino.Serialization;
using Sirenix.OdinInspector;

namespace TaoTie
{
    [NinoSerialize]
    public class StoryI18NText: StoryText
    {
        [NinoMember(1)][LabelText("多语言表Key")]
        public string I18NKey;
    }
}