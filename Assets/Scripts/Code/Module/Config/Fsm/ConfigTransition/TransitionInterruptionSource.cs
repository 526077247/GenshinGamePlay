using Sirenix.OdinInspector;

namespace TaoTie
{
    public enum TransitionInterruptionSource
    {
        [LabelText("不能中断")]
        None,
        [LabelText("可以被源动画中的过渡中断")]
        Source,
        [LabelText("可以被目标动画中的过渡中断")]
        Destination,
        [LabelText("可以被源(优先)和目标动画中的过渡中断")]
        SourceThenDestination,
        [LabelText("可以被目标(优先)和源动画中的过渡中断")]
        DestinationThenSource,
    }
}