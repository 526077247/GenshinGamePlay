using Sirenix.OdinInspector;

namespace TaoTie
{
    public enum StackingType
    {
        [LabelText("同时存在唯一一个")]
        Unique,
        [LabelText("互相独立存在")]
        Multiple,
        [LabelText("刷新已存在的modifier")]
        Refresh,
        [LabelText("延长已存在的modifier")]
        Prolong,
    }
}