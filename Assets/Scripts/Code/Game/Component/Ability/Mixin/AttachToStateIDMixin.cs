namespace TaoTie
{
    public class AttachToStateIDMixin : AbilityMixin
    {
        public ConfigAttachToStateIDMixin ConfigAttachTo => baseConfig as ConfigAttachToStateIDMixin;

        private Fsm fsm;
        private bool hasAddModifier;
        private Entity owner;
        private AbilityComponent abilityComponent;
        public override void Init(ActorAbility actorAbility, ActorModifier actorModifier, ConfigAbilityMixin config)
        {
            base.Init(actorAbility, actorModifier, config);
            owner = actorAbility.Parent.GetParent<Entity>();
            fsm = owner?.GetComponent<FsmComponent>()?.GetFsm(this.ConfigAttachTo.ChargeLayer);
            abilityComponent = owner?.GetComponent<AbilityComponent>();
            if (fsm != null)
            {
                fsm.onStateChanged += OnStateChanged;
                if (this.ConfigAttachTo.StateIDs.Contains(fsm.currentStateName))
                {
                    ApplyModifier();
                }
            }
        }

        private void OnStateChanged(string from, string to)
        {
            if (ConfigAttachTo.StateIDs == null || abilityComponent == null)
                return;

            bool flag = ConfigAttachTo.StateIDs.Contains(from), flag2 = ConfigAttachTo.StateIDs.Contains(to);
            if (!flag && flag2)
            {
                ApplyModifier();
            }
            else if (flag && !flag2)
            {
                RemoveModifier();
            }
        }

        private void ApplyModifier()
        {
            if (EvaluatePredicate())
            {
                abilityComponent.ApplyModifier(owner.Id, actorAbility, ConfigAttachTo.ModifierName);
                hasAddModifier = true;
            }
        }

        private bool EvaluatePredicate()
        {
            if (ConfigAttachTo.Predicate != null)
            {
                return ConfigAttachTo.Predicate.Evaluate(owner, actorAbility, actorModifier, owner);
            }
            return true;
        }

        private void RemoveModifier()
        {
            if (hasAddModifier)
            {
                abilityComponent.RemoveModifier(actorAbility.Config.AbilityName, ConfigAttachTo.ModifierName);
            }

            hasAddModifier = false;
        }

        public override void Dispose()
        {
            if (fsm != null)
            {
                fsm.onStateChanged -= OnStateChanged;
                fsm = null;
            }

            hasAddModifier = false;
            abilityComponent = null;
            owner = null;
            base.Dispose();
        }
    }
}