using System.Collections.Generic;
using ProtoBuf;
using Sirenix.OdinInspector;

namespace TaoTie
{
    [ProtoContract]
    public class ConfigStoryContentText: ConfigStoryText
    {
        [ProtoMember(1)][LabelText("默认内容")]
        public string Default;
        [ProtoMember(2, IsRequired = true)][LabelText("多语言")]
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