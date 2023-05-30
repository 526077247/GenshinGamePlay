namespace TaoTie
{
    public class EnterZoneEvent: IEventBase
    {
        public long EntityId;
        public long ZoneEntityId;
        [SceneGroupZoneId]
        public int ZoneLocalId;
    }
}