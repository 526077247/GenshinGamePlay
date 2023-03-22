using Nino.Serialization;

namespace TaoTie
{
    public abstract partial class BaseSceneGroupValue
    {
        public abstract float Resolve(IEventBase obj, DynDictionary set);
    }
}