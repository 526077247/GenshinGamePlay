namespace TaoTie
{
    public class DoActionByStateIDMixin : AbilityMixin<ConfigDoActionByStateIDMixin>
    {
        
        private Fsm fsm;
        private Entity owner;

        protected override void InitInternal(ActorAbility actorAbility, ActorModifier actorModifier, ConfigDoActionByStateIDMixin config)
        {
            owner = actorAbility.Parent.GetParent<Entity>();
            fsm = owner?.GetComponent<FsmComponent>()?.GetFsm(this.Config.ChargeLayer);
            
            if (fsm != null)
            {
                fsm.OnStateChanged += OnStateChanged;
                if (this.Config.StateIDs.Contains(fsm.CurrentStateName))
                {
                    OnEnter();
                }
            }
        }

        private void OnStateChanged(string from, string to)
        {
            if (Config.StateIDs == null)
                return;

            bool flag = Config.StateIDs.Contains(from), flag2 = Config.StateIDs.Contains(to);
            if (!flag && flag2)
            {
                OnEnter();
            }
            else if (flag && !flag2)
            {
                OnExit();
            }
        }

        private void OnEnter()
        {
            var actions = Config.EnterActions;
            if (actions!=null && EvaluatePredicate(Config.EnterPredicate))
            {
                var executer = GetActionExecuter();
                for (int i = 0; i < actions.Length; i++)
                {
                    actions[i].DoExecute(executer, actorAbility, actorModifier, executer);
                }
            }
        }

        private bool EvaluatePredicate(ConfigAbilityPredicate predicate)
        {
            if (predicate != null)
            {
                var executer = GetActionExecuter();
                return predicate.Evaluate(executer, actorAbility, actorModifier, executer);
            }
            return true;
        }

        private void OnExit()
        {
            var actions = Config.ExitActions;
            if (actions!=null && EvaluatePredicate(Config.ExitPredicate))
            {
                var executer = GetActionExecuter();
                for (int i = 0; i < actions.Length; i++)
                {
                    actions[i].DoExecute(executer, actorAbility, actorModifier, executer);
                }
            }
        }

        protected override void DisposeInternal()
        {
            if (fsm != null)
            {
                fsm.OnStateChanged -= OnStateChanged;
                fsm = null;
            }
            
            owner = null;
        }
    }
}