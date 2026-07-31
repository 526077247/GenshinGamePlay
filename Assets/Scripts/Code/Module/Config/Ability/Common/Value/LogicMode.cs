#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using TaoTie.Inspector;
#endif

namespace TaoTie
{
    public enum LogicMode
    {
        [LabelText("无")] Default,
        [LabelText("加")] Add,
        [LabelText("减")] Red,
        [LabelText("乘")] Mul,
        [LabelText("除")] Div,
        [LabelText("取余")] Rem,
        [LabelText("次方")] Pow,
    }
    
}