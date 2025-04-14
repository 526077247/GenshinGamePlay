using UnityEngine;

namespace TaoTie
{
    public class DoActionOnColliderBoxMixin : AbilityMixin<ConfigDoActionOnColliderBoxMixin>
    {
        private Entity owner;
        private ListComponent<GameObjectHolder> addHolders;
        protected override void InitInternal(ActorAbility actorAbility, ActorModifier actorModifier, ConfigDoActionOnColliderBoxMixin config)
        {
            addHolders = ListComponent<GameObjectHolder>.Create();
            owner = actorAbility.Parent.GetParent<Entity>();
            var umv = owner.GetComponent<UnitModelComponent>();
            for (var node = umv.Holders.First; node!=null; node = node.Next)
            {
                OnHolderCountChange(node.Value,true);
            }
            
            Messager.Instance.AddListener<GameObjectHolder,bool>(owner.Id, MessageId.OnHolderCountChange, OnHolderCountChange);
        }

        private async ETTask AddAsync(GameObjectHolder holder)
        {
            await holder.WaitLoadGameObjectOver();
            if (owner == null || addHolders == null) return;
            
            var components = holder.EntityView.GetComponentsInChildren<ColliderBoxComponent>(true);
            for (int i = 0; i < components.Length; i++)
            {
                components[i].OnTriggerEnterEvt += ExecuteTriggerEnter;
                components[i].OnTriggerExitEvt += ExecuteTriggerExit;
                // components[i].onTriggerStayEvt += ExecuteTriggerStay;
            }
        }
        private void RemoveSync(GameObjectHolder holder)
        {
            if (holder.EntityView == null) return;
            var components = holder.EntityView.GetComponentsInChildren<ColliderBoxComponent>(true);
            for (int i = 0; i < components.Length; i++)
            {
                components[i].OnTriggerEnterEvt -= ExecuteTriggerEnter;
                components[i].OnTriggerExitEvt -= ExecuteTriggerExit;
                // components[i].onTriggerStayEvt -= ExecuteTriggerStay;
            }
        }
        private void OnHolderCountChange(GameObjectHolder holder,bool isAdd)
        {
            if (isAdd)
            {
                addHolders.Add(holder);
                AddAsync(holder).Coroutine();
            }
            else if (addHolders.Contains(holder))
            {
                RemoveSync(holder);
                addHolders.Remove(holder);
            }
        }
        
        private void ExecuteTriggerEnter(Collider other)
        {
            var actions = Config.TriggerEnterActions;
            if (actions != null)
            {
                var executer = GetActionExecuter();
                if (other == null || other.transform == null) return;
                var gitBox = other.GetComponent<HitBoxComponent>();
                if (gitBox == null) return;
                var e = other.transform.GetComponentInParent<EntityComponent>();
                if (e == null) return;
                var target = executer.Parent.Get<Entity>(e.Id);
                for (int i = 0; i < actions.Length; i++)
                {
                    actions[i].DoExecute(executer, actorAbility, actorModifier, target);
                }
            }
        }
        private void ExecuteTriggerExit(Collider other)
        {
            var actions = Config.TriggerExitActions;
            if (actions != null)
            {
                var executer = GetActionExecuter();
                if (other == null || other.transform == null) return;
                var gitBox = other.GetComponent<HitBoxComponent>();
                if (gitBox == null) return;
                var e = other.transform.GetComponentInParent<EntityComponent>();
                if (e == null) return;
                var target = executer.Parent.Get<Entity>(e.Id);
                for (int i = 0; i < actions.Length; i++)
                {
                    actions[i].DoExecute(executer, actorAbility, actorModifier, target);
                }
            }
        }
        
        // private void ExecuteTriggerStay(Collider other)
        // {
        //     var actions = Config.TriggerStayActions;
        //     if (actions != null)
        //     {
        //         var owner = GetApplier();
        //         if (other == null || other.transform == null) return;
        //         var gitBox = other.GetComponent<HitBoxComponent>();
        //         if (gitBox == null) return;
        //         var e = other.transform.GetComponentInParent<EntityComponent>();
        //         if (e == null) return;
        //         var target = owner.Parent.Get<Entity>(e.Id);
        //         for (int i = 0; i < actions.Length; i++)
        //         {
        //             actions[i].DoExecute(owner, actorAbility, actorModifier, target);
        //         }
        //     }
        // }

        protected override void DisposeInternal()
        {
            Messager.Instance.RemoveListener<GameObjectHolder,bool>(owner.Id, MessageId.OnHolderCountChange,OnHolderCountChange);
            if (addHolders != null)
            {
                for (int i = 0; i < addHolders.Count; i++)
                {
                    RemoveSync(addHolders[i]);
                }
                addHolders.Dispose();
                addHolders = null;
            }
            
            owner = null;
        }
    }
}