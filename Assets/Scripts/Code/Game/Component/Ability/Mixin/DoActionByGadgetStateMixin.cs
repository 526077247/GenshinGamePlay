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
                var executer = GetActionExecuter();
                for (int i = 0; i < Config.EnterActions.Length; i++)
                {
                    Config.EnterActions[i].DoExecute(executer, actorAbility, actorModifier, executer);
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
            if (Config.ExitActions!=null && EvaluatePredicate(Config.ExitPredicate))
            {
                var executer = GetActionExecuter();
                for (int i = 0; i < Config.ExitActions.Length; i++)
                {
                    Config.ExitActions[i].DoExecute(executer, actorAbility, actorModifier, executer);
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