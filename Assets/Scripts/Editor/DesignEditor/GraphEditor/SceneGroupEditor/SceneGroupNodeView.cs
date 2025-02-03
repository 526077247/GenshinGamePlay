using System.Reflection;
using DaGenGraph;
using DaGenGraph.Editor;

namespace TaoTie
{
    public class SceneGroupNodeView: NodeView
    {
        protected override void RefreshValueDropDown(FieldInfo field, object obj, string valuesGetter)
        {
            var graph = this.graph as SceneGroupGraph;
            if (ValueDropDownHelper.RefreshValueDropDown(graph, field, obj, valuesGetter, valueDropdown))
            {
                return;
            }
            base.RefreshValueDropDown(field, obj, valuesGetter);
        }
    }
    
    public abstract class SceneGroupNodeView<T> : SceneGroupNodeView where T : NodeBase
    {
        public T node => base.node as T;
    }
}