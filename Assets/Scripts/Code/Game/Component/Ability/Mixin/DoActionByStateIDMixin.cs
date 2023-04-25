namespace TaoTie
{
    public class DoActionByStateIDMixin : AbilityMixin
    {
        public ConfigDoActionByStateIDMixin ConfigDoAction => baseConfig as ConfigDoActionByStateIDMixin;
        
        private Fsm fsm;
        private Entity owner;
        public override void Init(ActorAbility actorAbility, ActorModifier actorModifier, ConfigAbilityMixin config)
        {
            base.Init(actorAbility, actorModifier, config);
            owner = actorAbility.Parent.GetParent<Entity>();
            fsm = owner?.GetComponent<FsmComponent>()?.GetFsm(this.ConfigDoAction.ChargeLayer);
            
            if (fsm != null)
            {
                fsm.onStateChanged += OnStateChanged;
                if (this.ConfigDoAction.StateIDs.Contains(fsm.currentStateName))
                {
                    OnEnter();
                }
            }
        }

        private void OnStateChanged(string from, string to)
        {
            if (ConfigDoAction.StateIDs == null)
                return;

            bool flag = ConfigDoAction.StateIDs.Contains(from), flag2 = ConfigDoAction.StateIDs.Contains(to);
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
            if (ConfigDoAction.EnterActions!=null && EvaluatePredicate(ConfigDoAction.EnterPredicate))
            {
                for (int i = 0; i < ConfigDoAction.EnterActions.Length; i++)
                {
                    ConfigDoAction.EnterActions[i].DoExecute(owner, actorAbility, actorModifier, null);
                }
            }
        }

        private bool EvaluatePredicate(ConfigAbilityPredicate predicate)
        {
            if (predicate != null)
            {
                return predicate.Evaluate(owner, actorAbility, actorModifier, owner);
            }
            return true;
        }

        private void OnExit()
        {
            if (ConfigDoAction.ExitActions!=null && EvaluatePredicate(ConfigDoAction.ExitPredicate))
            {
                for (int i = 0; i < ConfigDoAction.ExitActions.Length; i++)
                {
                    ConfigDoAction.ExitActions[i].DoExecute(owner, actorAbility, actorModifier, null);
                }
            }
        }

        public override void Dispose()
        {
            if (fsm != null)
            {
                fsm.onStateChanged -= OnStateChanged;
                fsm = null;
            }
            
            owner = null;
            base.Dispose();
        }
    }
}