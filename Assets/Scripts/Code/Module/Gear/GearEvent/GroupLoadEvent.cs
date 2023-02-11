namespace TaoTie
{
    public struct GroupLoadEvent: IEventBase
    {
        [GearGroupId]
        public int GroupId;
        public bool IsAddOn;
    }
}