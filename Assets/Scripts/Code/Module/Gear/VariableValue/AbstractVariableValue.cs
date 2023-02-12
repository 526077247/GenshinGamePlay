using Nino.Serialization;

namespace TaoTie
{
    public abstract partial class AbstractVariableValue
    {
        public abstract float Resolve(IEventBase obj, VariableSet set);
    }
}