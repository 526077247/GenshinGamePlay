using System;

namespace TaoTie
{
    public class AttachToNormalizedTimeMixin: AbilityMixin
    {
        [Timer(TimerType.AttachToNormalizedTimeMixinUpdate)]
        public class AttachToNormalizedTimeMixinUpdate : ATimer<AttachToNormalizedTimeMixin>
        {
            public override void Run(AttachToNormalizedTimeMixin t)
            {
                try
                {
                    t.Update();
                }
                catch (Exception ex)
                {
                    Log.Error(ex);
                }
            }
        }
        public ConfigAttachToNormalizedTimeMixin ConfigAttachTo => baseConfig as ConfigAttachToNormalizedTimeMixin;

        private Fsm fsm;
        private bool hasAddModifier;
        private Entity owner;
        private AbilityComponent abilityComponent;
        private long timerId;
        public override void Init(ActorAbility actorAbility, ActorModifier actorModifier, ConfigAbilityMixin config)
        {
            base.Init(actorAbility, actorModifier, config);
            owner = actorAbility.Parent.GetParent<Entity>();
            fsm = owner?.GetComponent<FsmComponent>()?.GetFsm(this.ConfigAttachTo.ChargeLayer);
            abilityComponent = owner?.GetComponent<AbilityComponent>();
            if (fsm != null)
            {
                fsm.onStateChanged += OnStateChanged;
            }
        }

        private void OnStateChanged(string from, string to)
        {
            if (from == ConfigAttachTo.ModifierName)
            {
                GameTimerManager.Instance.Remove(ref timerId);
                RemoveModifier();
            }
            else if (to == ConfigAttachTo.ModifierName)
            {
                timerId = GameTimerManager.Instance.NewFrameTimer(TimerType.AttachToNormalizedTimeMixinUpdate, this);
            }
        }

        private void Update()
        {
            if (fsm.stateNormalizedTime > ConfigAttachTo.normalizeStartRawNum)
            {
                ApplyModifier();
            }
            else if (fsm.stateNormalizedTime > ConfigAttachTo.normalizeEndRawNum)
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