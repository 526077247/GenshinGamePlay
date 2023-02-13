using Sirenix.OdinInspector;

namespace TaoTie
{
    public enum RouteType:byte
    {
        [LabelText("一次性")]
        OneWay = 0,
        [LabelText("来回")]
        Reciprocate = 1,
        [LabelText("循环")]
        Loop = 2
    }
}