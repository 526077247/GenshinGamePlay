using Sirenix.OdinInspector;
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