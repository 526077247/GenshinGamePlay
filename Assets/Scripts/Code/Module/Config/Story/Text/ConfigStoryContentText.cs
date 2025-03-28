using System.Collections.Generic;
using Nino.Core;
using Sirenix.OdinInspector;

namespace TaoTie
{
    [NinoType(false)]
    public class ConfigStoryContentText: ConfigStoryText
    {
        [NinoMember(1)][LabelText("默认内容")]
        public string Default;
        [NinoMember(2)][LabelText("多语言")]
        public Dictionary<LangType, string> Others = new Dictionary<LangType, string>();

        public override string GetShowText()
        {
            if (Others!=null && Others.TryGetValue(I18NManager.Instance.CurLangType, out var txt))
            {
                return txt;
            }
            return Default;
        }
    }
}