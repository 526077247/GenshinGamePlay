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
                if (fsm.CurrentStateName == config.StateID) StartTimer();
            }
        }
        private void StartTimer()
        {
            if (timerId != 0 || fsm == null) return;
            timerId = GameTimerManager.Instance.NewFrameTimer(TimerType.AttachToNormalizedTimeMixinUpdate, this);
            Update();
        }
        private void StopTimer() => GameTimerManager.Instance.Remove(ref timerId);
        private void OnStateChanged(string from, string to)
        {
            if (abilityComponent == null) return;
            bool f = from == Config.StateID, t = to == Config.StateID;
            if (!f && t)
            {
                StartTimer();
            }
            else if (f && !t)
            {
                StopTimer();
                RemoveModifier();
            }
        }

        private void Update()
        {
            if (fsm == null || abilityComponent == null) { StopTimer(); return; }
            float t = fsm.StateNormalizedTime;
            if (fsm.StateNormalizedTime > Config.normalizeStartRawNum  && t <= Config.normalizeEndRawNum&& !hasAddModifier)
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
            if (hasAddModifier) return;
            if (!EvaluatePredicate()) return;
            if (abilityComponent.ApplyModifier(owner.Id, actorAbility, Config.ModifierName) != null)
                hasAddModifier = true;   
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
            if (!hasAddModifier) return;
            hasAddModifier = false;
            abilityComponent.RemoveModifier(actorAbility.Config.AbilityName, Config.ModifierName);
        }

        protected override void DisposeInternal()
        {
            if (fsm != null)
            {
                fsm.OnStateChanged -= OnStateChanged;
                fsm = null;
            }
            StopTimer();
            RemoveModifier();
            abilityComponent = null;
            owner = null;
        }
    }
}