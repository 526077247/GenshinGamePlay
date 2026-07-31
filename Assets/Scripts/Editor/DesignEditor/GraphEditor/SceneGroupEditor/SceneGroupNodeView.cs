using System.Reflection;
using TaoTie.Inspector.Editor;
using TaoTie.Inspector;
namespace TaoTie
{
    public class SceneGroupNodeView: NodeView
    {
        protected override DrawBase CreateDrawBase()
        {
            return new SceneGroupGraphDrawBase(this.graphWindow as SceneGroupGraphWindow);
        }
    }
    
    public abstract class SceneGroupNodeView<T> : SceneGroupNodeView where T : NodeBase
    {
        public T node => base.node as T;
    }
}