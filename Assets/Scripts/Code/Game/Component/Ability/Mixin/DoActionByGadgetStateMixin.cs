namespace TaoTie
{
    public class DoActionByGadgetStateMixin: AbilityMixin
    {
        public ConfigDoActionByGadgetStateMixin config => baseConfig as ConfigDoActionByGadgetStateMixin;

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
                if (this.config.StateIDs.Contains(gadgetComponent.GadgetState))
                {
                    OnEnter();
                }
            }
        }

        private void OnStateChanged(GadgetState from, GadgetState to)
        {
            if (config.StateIDs == null)
                return;

            bool flag = config.StateIDs.Contains(from), flag2 = config.StateIDs.Contains(to);
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
            if (config.EnterActions!=null && EvaluatePredicate(config.EnterPredicate))
            {
                for (int i = 0; i < config.EnterActions.Length; i++)
                {
                    config.EnterActions[i].DoExecute(owner, actorAbility, actorModifier, null);
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
            if (config.ExitActions!=null && EvaluatePredicate(config.ExitPredicate))
            {
                for (int i = 0; i < config.ExitActions.Length; i++)
                {
                    config.ExitActions[i].DoExecute(owner, actorAbility, actorModifier, null);
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