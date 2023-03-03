namespace TaoTie
{
    public struct SuiteLoadEvent: IEventBase
    {
        [SceneGroupSuiteId]
        public int SuiteId;
        public bool IsAddOn;
    }
}