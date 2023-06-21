using Nino.Serialization;
using Sirenix.OdinInspector;

namespace TaoTie
{
    [NinoSerialize][LabelText("等待时间")]
    public class ConfigStoryWaitTimeClip: ConfigStoryClip
    {
        [NinoMember(10)][LabelText("时间间隔ms")]
        public int Interval;

        [NinoMember(11)]
        public bool IsGameTime = true;
        public override async ETTask Process()
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