namespace TaoTie
{
    public struct EnterZoneEvent: IEventBase
    {
        public long EntityId;
        public long ZoneEntityId;
        [SceneGroupZoneId]
        public int ZoneLocalId;
    }
}