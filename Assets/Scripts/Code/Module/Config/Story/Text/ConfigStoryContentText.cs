using System.Collections.Generic;
using Nino.Serialization;
using Sirenix.OdinInspector;

namespace TaoTie
{
    [NinoSerialize]
    public class ConfigStoryContentText: ConfigStoryText
    {
        [NinoMember(1)][LabelText("默认内容")]
        public string Default;
        [NinoMember(2)][LabelText("多语言")]
        public Dictionary<LangType, string> Others;

        public override string GetShowText()
        {
            if (Others.TryGetValue(I18NManager.Instance.CurLangType, out var txt))
            {
                return txt;
            }
            return Default;
        }
    }
}