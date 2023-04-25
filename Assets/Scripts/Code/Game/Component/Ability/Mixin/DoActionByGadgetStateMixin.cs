namespace TaoTie
{
    public class DoActionByGadgetStateMixin: AbilityMixin
    {
        public ConfigDoActionByGadgetStateMixin ConfigDoAction => baseConfig as ConfigDoActionByGadgetStateMixin;

        private GadgetComponent gadgetComponent;
        private Entity owner;
        public override void Init(ActorAbility actorAbility, ActorModifier actorModifier, ConfigAbilityMixin config)
        {
            base.Init(actorAbility, actorModifier, config);
            owner = actorAbility.Parent.GetParent<Entity>();
            gadgetComponent = owner?.GetComponent<GadgetComponent>();
            
            if (gadgetComponent != null)
            {
                gadgetComponent.onGadgetStateChange += OnStateChanged;
                if (this.ConfigDoAction.StateIDs.Contains(gadgetComponent.GadgetState))
                {
                    OnEnter();
                }
            }
        }

        private void OnStateChanged(GadgetState from, GadgetState to)
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
            if (gadgetComponent != null)
            {
                gadgetComponent.onGadgetStateChange -= OnStateChanged;
                gadgetComponent = null;
            }
            
            owner = null;
            base.Dispose();
        }
    }
}