using System;

namespace TaoTie
{
    public abstract class ConfigCondition
    {
        public enum CompareMode
        {
            Equal,
            NotEqual,
            Greater,
            Less,
            LEqual,
            GEqual,
        }

        public abstract ConfigCondition Copy();
        public abstract bool IsMatch(Fsm fsm);
        public virtual void OnTransitionApply(Fsm fsm) { }
    }
}