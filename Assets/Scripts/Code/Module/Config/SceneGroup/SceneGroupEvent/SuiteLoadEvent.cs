using Sirenix.OdinInspector;
namespace TaoTie
{
    public class SuiteLoadEvent: IEventBase
    {
        [LabelText("组Id")]
        [SceneGroupSuiteId]
        public int SuiteId;
        [LabelText("附加(true)还是替换(false)")]
        public bool IsAddOn;
    }
}