namespace DaGenGraph
{
    public abstract class JsonNodeBase:NodeBase
    {
        protected override Port CreatePortBase()
        {
            var node = CreateInstance<Port>();
            node.name = "Port";
            return node;
        }

        protected override void DeletePortBase(Port port)
        {
            
        }
    }
}