namespace TaoTie
{
    public struct ExitZoneEvent: IEventBase
    {
        public long EntityId;
        public long ZoneEntityId;
        public long ZoneLocalId;
    }
}