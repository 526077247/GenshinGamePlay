using Nino.Serialization;
using Sirenix.OdinInspector;

namespace TaoTie
{
    [LabelText("串行执行")][NinoSerialize]
    public class SerialStoryClipConfig: StoryClipConfig
    {
        [NinoMember(10)]
        public StoryClipConfig[] Clips;
    }
}