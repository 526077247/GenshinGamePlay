using Nino.Serialization;

namespace TaoTie
{
    public abstract partial class BaseGearValue
    {
        public abstract float Resolve(IEventBase obj, VariableSet set);
    }
}