using DaGenGraph.Editor;
using UnityEditor;

namespace TaoTie
{
    public class AINodeView:NodeView
    {
        protected override GraphWindow GetWindow()
        {
            return AIGraphWindow.initance;
        }
    }
}