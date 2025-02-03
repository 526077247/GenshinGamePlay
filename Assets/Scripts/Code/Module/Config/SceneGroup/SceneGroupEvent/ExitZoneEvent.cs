using Sirenix.OdinInspector;
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