namespace TaoTie
{
    public class ExitZoneEvent: IEventBase
    {
        public long EntityId;
        public long ZoneEntityId;
        [SceneGroupZoneId]
        public int ZoneLocalId;
    }
}