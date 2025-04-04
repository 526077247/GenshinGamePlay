﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace TaoTie
{
    public class AbilitySystem: IManager
    {
        public static AbilitySystem Instance;
        private Dictionary<Type, Type> configMixinType;
        
        public void Init()
        {
            Instance = this;
            configMixinType = new Dictionary<Type, Type>();
            var allTypes = AssemblyManager.Instance.GetTypes();
            var runnerType = TypeInfo<AbilityMixin>.Type;
            foreach (var item in allTypes)
            {
                var type = item.Value;
                if (!type.IsAbstract && runnerType.IsAssignableFrom(type))
                {
                    configMixinType.Add(type.BaseType.GenericTypeArguments[0],type);
                }
            }
        }

        public void Destroy()
        {
            configMixinType = null;
            Instance = null;
        }
        /// <summary>
        /// 创建Mixin
        /// </summary>
        /// <param name="actorAbility"></param>
        /// <param name="actorModifier"></param>
        /// <param name="config"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public AbilityMixin CreateAbilityMixin<T>(ActorAbility actorAbility, ActorModifier actorModifier,T config) where T:ConfigAbilityMixin
        {
            if(configMixinType.TryGetValue(config.GetType(), out var type))
            {
                var res = ObjectPool.Instance.Fetch(type) as AbilityMixin;
                res.Init(actorAbility,actorModifier,config);
                return res;
            }
            Log.Error($"CreateAbilityMixin失败， 未实现{config.GetType()}对应的Mixin");
            return null;
        }
        /// <summary>
        /// 获取目标
        /// </summary>
        /// <param name="actor"></param>
        /// <param name="ability"></param>
        /// <param name="modifier"></param>
        /// <param name="target"></param>
        /// <param name="targetting"></param>
        /// <param name="otherTargets"></param>
        /// <returns></returns>
        public static int ResolveTarget(Entity actor, ActorAbility ability, ActorModifier modifier, Entity target,
            AbilityTargetting targetting, out Entity[] entities, ConfigSelectTargets otherTargets = null)
        {
            switch (targetting)
            {
                case AbilityTargetting.Self:
                    entities = new[] { actor };
                    return 1;
                case AbilityTargetting.Caster:
                    entities = new[] { ability.Parent.GetParent<Entity>() };
                    return 1;
                case AbilityTargetting.Target:
                    entities = new[] { target };
                    return 1;
                case AbilityTargetting.SelfAttackTarget:
                    entities = null;
                    return 0;
                case AbilityTargetting.Applier:
                    if (modifier != null)
                    {
                        var em = ability.Parent.GetParent<Entity>().Parent;
                        Entity applierEntity = em.Get<Entity>(modifier.ApplierID);
                        if (applierEntity != null)
                        {
                            entities = new[] { applierEntity };
                            return 1;
                        }
                    }

                    entities = null;
                    return 0;
                case AbilityTargetting.CurLocalAvatar:
                    var scene = SceneManager.Instance.GetCurrentScene<MapScene>();
                    if (scene != null)
                    {
                        entities = new[] { scene.Self };
                        return 1;
                    }
                    entities = null;
                    return 0;
                case AbilityTargetting.Other:
                    if (otherTargets == null)
                    {
                        entities = null;
                        return 0;
                    }
                    entities = otherTargets.ResolveTargets(actor, ability, modifier, target);
                    return entities.Length;
                default:
                    entities = null;
                    return 0;
            }
        }
        
        /// <summary>
        /// 获取目标
        /// </summary>
        /// <param name="actor"></param>
        /// <param name="ability"></param>
        /// <param name="modifier"></param>
        /// <param name="target"></param>
        /// <param name="targetting"></param>
        /// <returns></returns>
        public static Entity ResolveTarget(Entity actor, ActorAbility ability, ActorModifier modifier, Entity target,
                    AttachPointTargetType targetting)
        {
            switch (targetting)
            {
                case AttachPointTargetType.Self:
                    return actor;
                case AttachPointTargetType.Caster:
                    return ability.Parent.GetParent<Entity>();
                case AttachPointTargetType.Target:
                    return target;
                case AttachPointTargetType.Applier:
                    if (modifier != null)
                    {
                        var em = ability.Parent.GetParent<Entity>().Parent;
                        return em.Get<Entity>(modifier.ApplierID);
                    }
                    break;
            }
            return null;
        }
                
        public static bool IsTarget(Actor self, Actor other, TargetType type)
        {
            switch (type)
            {
                case TargetType.All:
                    return true;
                case TargetType.Self:
                    return self == other;
                case TargetType.Enemy:
                    return self.CampId != other.CampId;
                case TargetType.SelfCamp:
                    return self.CampId == other.CampId;
                case TargetType.AllExceptSelf:
                    return self != other;
                case TargetType.Alliance:
                    return CampManager.Instance.IsAlliances(self.CampId, other.CampId);
                default:
                    return false;
            }
        }
    }
}