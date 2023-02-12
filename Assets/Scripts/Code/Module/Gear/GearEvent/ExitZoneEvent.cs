namespace TaoTie
{
    public struct ExitZoneEvent: IEventBase
    {
        public long EntityId;
        public long ZoneEntityId;
        [GearZoneId]
        public long ZoneLocalId;
    }
}