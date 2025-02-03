using Sirenix.OdinInspector;
namespace TaoTie
{
    public class GameTimeChange: IEventBase
    {
        [LabelText("游戏时间(ms)")]
        public long GameTimeNow;
    }
}