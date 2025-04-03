namespace TaoTie
{
    public class DoActionByGadgetStateMixin: AbilityMixin<ConfigDoActionByGadgetStateMixin>
    {

        private GadgetComponent gadgetComponent;
        private Entity owner;

        protected override void InitInternal(ActorAbility actorAbility, ActorModifier actorModifier, ConfigDoActionByGadgetStateMixin config)
        {
            owner = actorAbility.Parent.GetParent<Entity>();
            gadgetComponent = owner?.GetComponent<GadgetComponent>();
            
            if (gadgetComponent != null)
            {
                gadgetComponent.onGadgetStateChange += OnStateChanged;
                if (this.Config.StateIDs.Contains(gadgetComponent.GadgetState))
                {
                    OnEnter();
                }
            }
        }

        private void OnStateChanged(GadgetState from, GadgetState to)
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

        protected override void DisposeInternal()
        {
            if (gadgetComponent != null)
            {
                gadgetComponent.onGadgetStateChange -= OnStateChanged;
                gadgetComponent = null;
            }
            
            owner = null;
        }
    }
}