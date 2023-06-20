using Nino.Serialization;
using Sirenix.OdinInspector;

namespace TaoTie
{
    [NinoSerialize][LabelText("等待时间")]
    public class WaitTimeStoryClipConfig: StoryClipConfig
    {
        [NinoMember(10)][LabelText("时间间隔ms")]
        public int Interval;
    }
}