namespace DaGenGraph
{
    public abstract class JsonNodeBase:NodeBase
    {
        protected override Port CreatePortBase<T>()
        {
            var node = CreateInstance<T>();
            node.name = "Port";
            return node;
        }

        protected override void DeletePortBase(Port port)
        {
            
        }
    }
}