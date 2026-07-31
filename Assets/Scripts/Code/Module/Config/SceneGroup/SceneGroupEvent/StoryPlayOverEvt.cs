#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using TaoTie.Inspector;
#endif
namespace TaoTie
{
    public class StoryPlayOverEvt: IEventBase
    {
        /// <summary>
        /// 播完的剧情
        /// </summary>
        [LabelText("播完的剧情id")]
        public ulong StoryId;
    }
}