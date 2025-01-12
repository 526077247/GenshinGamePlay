using Nino.Core;

namespace TaoTie
{
    [NinoType(false)]
    public abstract partial class BaseSceneGroupValue
    {
        public abstract float Resolve(IEventBase obj, DynDictionary set);
    }
}