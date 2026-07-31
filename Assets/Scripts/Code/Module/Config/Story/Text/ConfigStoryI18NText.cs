using ProtoBuf;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using TaoTie.Inspector;
#endif

namespace TaoTie
{
    [ProtoContract]
    public class ConfigStoryI18NText: ConfigStoryText
    {
        [ProtoMember(1)][LabelText("多语言表Key")]
        public string I18NKey;

        public override string GetShowText()
        {
            return I18NManager.Instance.I18NGetText(I18NKey);
        }
    }
}