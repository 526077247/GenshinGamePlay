#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using TaoTie.Inspector;
#endif
namespace TaoTie
{
    public class GameTimeChange: IEventBase
    {
        [LabelText("游戏时间(ms)")]
        public long GameTimeNow;
    }
}