using Sirenix.OdinInspector;

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