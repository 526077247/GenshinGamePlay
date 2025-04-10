﻿using UnityEngine;

namespace TaoTie
{
    public class DoActionOnTriggerBoxMixin : AbilityMixin<ConfigDoActionOnTriggerBoxMixin>
    {
        private Entity owner;
        private ListComponent<GameObjectHolder> addHolders;
        protected override void InitInternal(ActorAbility actorAbility, ActorModifier actorModifier, ConfigDoActionOnTriggerBoxMixin config)
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
            
            var components = holder.EntityView.GetComponentsInChildren<TriggerBoxComponent>(true);
            for (int i = 0; i < components.Length; i++)
            {
                components[i].onTriggerEnterEvt += ExecuteTriggerEnter;
                components[i].onTriggerExitEvt += ExecuteTriggerExit;
                // components[i].onTriggerStayEvt += ExecuteTriggerStay;
            }
        }
        private void RemoveSync(GameObjectHolder holder)
        {
            if (holder.EntityView == null) return;
            var components = holder.EntityView.GetComponentsInChildren<TriggerBoxComponent>(true);
            for (int i = 0; i < components.Length; i++)
            {
                components[i].onTriggerEnterEvt -= ExecuteTriggerEnter;
                components[i].onTriggerExitEvt -= ExecuteTriggerExit;
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