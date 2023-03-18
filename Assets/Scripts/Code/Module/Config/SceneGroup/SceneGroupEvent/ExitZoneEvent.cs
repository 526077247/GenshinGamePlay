namespace TaoTie
{
    public struct ExitZoneEvent: IEventBase
    {
        public long EntityId;
        public long ZoneEntityId;
        [SceneGroupZoneId]
        public int ZoneLocalId;
    }
}