namespace TaoTie
{
    public struct EnterZoneEvent: IEventBase
    {
        public long EntityId;
        public long ZoneEntityId;
        [GearZoneId]
        public int ZoneLocalId;
    }
}