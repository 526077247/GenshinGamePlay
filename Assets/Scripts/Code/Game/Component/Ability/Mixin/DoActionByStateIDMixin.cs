namespace TaoTie
{
    public class DoActionByStateIDMixin : AbilityMixin
    {
        public ConfigDoActionByStateIDMixin Config => baseConfig as ConfigDoActionByStateIDMixin;
        
        private Fsm fsm;
        private Entity owner;
        public override void Init(ActorAbility actorAbility, ActorModifier actorModifier, ConfigAbilityMixin config)
        {
            base.Init(actorAbility, actorModifier, config);
            owner = actorAbility.Parent.GetParent<Entity>();
            fsm = owner?.GetComponent<FsmComponent>()?.GetFsm(this.Config.ChargeLayer);
            
            if (fsm != null)
            {
                fsm.onStateChanged += OnStateChanged;
                if (this.Config.StateIDs.Contains(fsm.currentStateName))
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
            if (Config.EnterActions!=null && EvaluatePredicate(Config.EnterPredicate))
            {
                for (int i = 0; i < Config.EnterActions.Length; i++)
                {
                    Config.EnterActions[i].DoExecute(owner, actorAbility, actorModifier, null);
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
            if (Config.ExitActions!=null && EvaluatePredicate(Config.ExitPredicate))
            {
                for (int i = 0; i < Config.ExitActions.Length; i++)
                {
                    Config.ExitActions[i].DoExecute(owner, actorAbility, actorModifier, null);
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