using DaGenGraph.Editor;

namespace DaGenGraph.Example
{
    public class ExampleNodeView : NodeView<ExampleNode>
    {
        public override float DrawInspector(bool isDetails = false)
        {
            return base.DrawInspector(isDetails);
        }
    }
}