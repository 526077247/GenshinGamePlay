using System.Collections.Generic;
using Nino.Serialization;
using Sirenix.OdinInspector;

namespace TaoTie
{
    [NinoSerialize]
    public class StoryContentText: StoryText
    {
        [NinoMember(1)][LabelText("默认内容")]
        public string Default;
        [NinoMember(2)][LabelText("多语言")]
        public Dictionary<LangType, string> Others;
    }
}