using System;

namespace TaoTie
{
    public class AttachToNormalizedTimeMixin:  AbilityMixin<ConfigAttachToNormalizedTimeMixin>
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

        private Fsm fsm;
        private bool hasAddModifier;
        private Entity owner;
        private AbilityComponent abilityComponent;
        private long timerId;

        protected override void InitInternal(ActorAbility actorAbility, ActorModifier actorModifier, ConfigAttachToNormalizedTimeMixin config)
        {
            owner = actorAbility.Parent.GetParent<Entity>();
            fsm = owner?.GetComponent<FsmComponent>()?.GetFsm(this.Config.ChargeLayer);
            abilityComponent = owner?.GetComponent<AbilityComponent>();
            if (fsm != null)
            {
                fsm.OnStateChanged += OnStateChanged;
            }
        }

        private void OnStateChanged(string from, string to)
        {
            if (from == Config.ModifierName)
            {
                GameTimerManager.Instance.Remove(ref timerId);
                RemoveModifier();
            }
            else if (to == Config.ModifierName)
            {
                timerId = GameTimerManager.Instance.NewFrameTimer(TimerType.AttachToNormalizedTimeMixinUpdate, this);
            }
        }

        private void Update()
        {
            if (fsm.StateNormalizedTime > Config.normalizeStartRawNum)
            {
                ApplyModifier();
            }
            else if (fsm.StateNormalizedTime > Config.normalizeEndRawNum)
            {
                RemoveModifier();
            }
        }

        private void ApplyModifier()
        {
            if (EvaluatePredicate())
            {
                abilityComponent.ApplyModifier(owner.Id, actorAbility, Config.ModifierName);
                hasAddModifier = true;
            }
        }

        private bool EvaluatePredicate()
        {
            if (Config.Predicate != null)
            {
                return Config.Predicate.Evaluate(owner, actorAbility, actorModifier, owner);
            }
            return true;
        }

        private void RemoveModifier()
        {
            if (hasAddModifier)
            {
                abilityComponent.RemoveModifier(actorAbility.Config.AbilityName, Config.ModifierName);
            }

            hasAddModifier = false;
        }

        protected override void DisposeInternal()
        {
            if (fsm != null)
            {
                fsm.OnStateChanged -= OnStateChanged;
                fsm = null;
            }

            hasAddModifier = false;
            abilityComponent = null;
            owner = null;
        }
    }
}