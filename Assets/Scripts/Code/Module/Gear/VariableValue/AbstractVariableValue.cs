using System;

namespace TaoTie
{

    [Serializable]
    public abstract class AbstractVariableValue
    {
        public abstract float Resolve(IEventBase obj, VariableSet set);
    }
}