namespace TaoTie
{
    public class SuiteLoadEvent: IEventBase
    {
        [SceneGroupSuiteId]
        public int SuiteId;
        public bool IsAddOn;
    }
}