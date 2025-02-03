using Sirenix.OdinInspector;
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