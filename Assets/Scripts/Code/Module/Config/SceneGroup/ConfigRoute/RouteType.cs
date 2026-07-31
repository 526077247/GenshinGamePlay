#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using TaoTie.Inspector;
#endif

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