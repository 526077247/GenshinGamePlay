using System;

namespace TaoTie
{
    public abstract class ConfigCondition
    {


        public abstract ConfigCondition Copy();
        public abstract bool IsMatch(Fsm fsm);
        public virtual void OnTransitionApply(Fsm fsm) { }
    }
}