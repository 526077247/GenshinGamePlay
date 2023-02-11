using Nino.Serialization;

namespace TaoTie
{
    [NinoSerialize]
    public abstract partial class AbstractVariableValue
    {
        public abstract float Resolve(IEventBase obj, VariableSet set);
    }
}