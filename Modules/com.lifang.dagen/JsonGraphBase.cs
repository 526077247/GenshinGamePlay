using System;

namespace DaGenGraph
{
    public abstract class JsonGraphBase: GraphBase
    {
        protected override T CreateNodeBase<T>()
        {
            var node = Activator.CreateInstance<T>() ;
            node.name = "Node";
            return node;
        }

        protected override void DeleteNodeBase<T>(T nodeBase)
        {
           
        }

        protected override Edge CreateEdgeBase()
        {
            var edge = Activator.CreateInstance<Edge>() ;
            edge.name = "Edge";
            return edge;
        }

        protected override void RemoveEdgeBase(Edge edge)
        {
            
        }
    }
}