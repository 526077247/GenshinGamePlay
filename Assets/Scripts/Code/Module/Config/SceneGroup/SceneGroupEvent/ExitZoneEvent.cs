#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using TaoTie.Inspector;
#endif
namespace TaoTie
{
    public class ExitZoneEvent: IEventBase
    {
        [SceneGroupGenerateIgnore]
        public long EntityId;
        [SceneGroupGenerateIgnore]
        public long ZoneEntityId;
        [SceneGroupZoneId]
        [LabelText("区域Id")]
        public int ZoneLocalId;
    }
}