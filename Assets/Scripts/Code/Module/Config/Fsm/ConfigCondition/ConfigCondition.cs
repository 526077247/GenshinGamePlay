using System;
using Nino.Core;

namespace TaoTie
{
    [NinoType(false)]
    public abstract partial class ConfigCondition
    {
        public abstract ConfigCondition Copy();
        public abstract bool IsMatch(Fsm fsm);
        public virtual void OnTransitionApply(Fsm fsm) { }

        public abstract bool Equals(ConfigCondition other);
    }
    [NinoType(false)]
    public abstract partial class ConfigConditionByData:ConfigCondition
    {
        [NinoMember(1)]
        public string Key;
        [NinoMember(3)]
        public CompareMode Mode;
    }
    [NinoType(false)]
    public abstract partial class ConfigConditionByData<T> :ConfigConditionByData  where T : unmanaged
    {
        [NinoMember(2)]
        public T Value;
    }
}