#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using TaoTie.Inspector;
#endif

namespace TaoTie
{
    public enum CompareMode
    {
        [LabelText("==")] Equal,
        [LabelText("!=")] NotEqual,
        [LabelText(">")] Greater,
        [LabelText("<")] Less,
        [LabelText("<=")] LEqual,
        [LabelText(">=")] GEqual,
    }
}