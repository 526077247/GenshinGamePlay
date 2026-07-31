#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using TaoTie.Inspector;
#endif
namespace TaoTie
{
    [SceneGroupGenerateIgnore]
    public class VariableChangeEvent: IEventBase
    {
        public string Key;
        [LabelText("原值")]
        public float OldValue;
        [LabelText("新值")]
        public float NewValue;
    }
}