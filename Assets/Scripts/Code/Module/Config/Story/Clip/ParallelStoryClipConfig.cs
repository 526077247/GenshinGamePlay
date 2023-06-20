using Nino.Serialization;
using Sirenix.OdinInspector;

namespace TaoTie
{
    [LabelText("并行执行")][NinoSerialize]
    public class ParallelStoryClipConfig: StoryClipConfig
    {
        [NinoMember(10)]
        public StoryClipConfig[] Clips;
    }
}