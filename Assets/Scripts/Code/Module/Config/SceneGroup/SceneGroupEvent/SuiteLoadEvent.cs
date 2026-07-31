#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using TaoTie.Inspector;
#endif
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