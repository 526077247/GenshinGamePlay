using System;
using System.Collections.Generic;
using UnityEngine;

namespace TaoTie
{
    public static class AbilityHelper
    {
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
                    var scene = SceneManager.Instance.GetCurrentScene<BaseMapScene>();
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
        
        public static bool IsTarget(Unit self, Unit other, TargetType type)
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
                    return true;
                default:
                    return false;
            }
        }
    }
}