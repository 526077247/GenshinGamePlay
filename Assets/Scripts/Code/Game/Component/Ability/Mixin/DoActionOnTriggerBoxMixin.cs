using UnityEngine;

namespace TaoTie
{
    public class DoActionOnTriggerBoxMixin : AbilityMixin<ConfigDoActionOnTriggerBoxMixin>
    {
        private Entity owner;
        private TriggerBoxComponent triggerBoxComponent;
        protected override void InitInternal(ActorAbility actorAbility, ActorModifier actorModifier, ConfigDoActionOnTriggerBoxMixin config)
        {
            owner = actorAbility.Parent.GetParent<Entity>();
            InitInternalAsync().Coroutine();
        }

        private async ETTask InitInternalAsync()
        {
            var umv = owner.GetComponent<UnitModelComponent>();
            await umv.WaitLoadGameObjectOver();
            if (owner == null || umv.IsDispose) return;
            triggerBoxComponent = umv.EntityView.GetComponentInChildren<TriggerBoxComponent>(true);
            if (triggerBoxComponent != null)
            {
                triggerBoxComponent.onTriggerEnterEvt += ExecuteTriggerEnter;
                triggerBoxComponent.onTriggerExitEvt += ExecuteTriggerExit;
                // triggerBoxComponent.onTriggerStayEvt += ExecuteTriggerStay;

            }
        }
        
        private void ExecuteTriggerEnter(Collider other)
        {
            if (Config.TriggerEnterActions != null)
            {
                var executer = GetActionExecuter();
                if (other == null || other.transform == null) return;
                var gitBox = other.GetComponent<HitBoxComponent>();
                if (gitBox == null) return;
                var e = other.transform.GetComponentInParent<EntityComponent>();
                if (e == null) return;
                var target = executer.Parent.Get<Entity>(e.Id);
                for (int i = 0; i < Config.TriggerEnterActions.Length; i++)
                {
                    Config.TriggerEnterActions[i].DoExecute(executer, actorAbility, actorModifier, target);
                }
            }
        }
        private void ExecuteTriggerExit(Collider other)
        {
            if (Config.TriggerExitActions != null)
            {
                var executer = GetActionExecuter();
                if (other == null || other.transform == null) return;
                var gitBox = other.GetComponent<HitBoxComponent>();
                if (gitBox == null) return;
                var e = other.transform.GetComponentInParent<EntityComponent>();
                if (e == null) return;
                var target = executer.Parent.Get<Entity>(e.Id);
                for (int i = 0; i < Config.TriggerExitActions.Length; i++)
                {
                    Config.TriggerExitActions[i].DoExecute(executer, actorAbility, actorModifier, target);
                }
            }
        }
        
        // private void ExecuteTriggerStay(Collider other)
        // {
        //     if (Config.TriggerStayActions != null)
        //     {
        //         var owner = GetApplier();
        //         if (other == null || other.transform == null) return;
        //         var gitBox = other.GetComponent<HitBoxComponent>();
        //         if (gitBox == null) return;
        //         var e = other.transform.GetComponentInParent<EntityComponent>();
        //         if (e == null) return;
        //         var target = owner.Parent.Get<Entity>(e.Id);
        //         for (int i = 0; i < Config.TriggerStayActions.Length; i++)
        //         {
        //             Config.TriggerStayActions[i].DoExecute(owner, actorAbility, actorModifier, target);
        //         }
        //     }
        // }

        protected override void DisposeInternal()
        {
            if (triggerBoxComponent != null)
            {
                triggerBoxComponent.onTriggerEnterEvt -= ExecuteTriggerEnter;
                triggerBoxComponent.onTriggerExitEvt -= ExecuteTriggerExit;
                // triggerBoxComponent.onTriggerStayEvt -= ExecuteTriggerStay;
                triggerBoxComponent = null;
            }
            owner = null;
        }
    }
}