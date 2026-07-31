using ProtoBuf;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using TaoTie.Inspector;
#endif

namespace TaoTie
{
    [ProtoContract][LabelText("等待时间")]
    public partial class ConfigStoryWaitTimeClip: ConfigStoryClip
    {
        [ProtoMember(10)][LabelText("时间间隔ms")]
        public int Interval;

        [ProtoMember(11, IsRequired = true)]
        public bool IsGameTime = true;
        public override async ETTask Process(StorySystem storySystem)
        {
            if (IsGameTime)
            {
                await GameTimerManager.Instance.WaitAsync(Interval);
            }
            else
            {
                await TimerManager.Instance.WaitAsync(Interval);
            }
        }
    }
}