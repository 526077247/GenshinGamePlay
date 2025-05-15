using UnityEngine;

namespace TaoTie
{
    public static class TargetHelper
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
        public static ListComponent<Entity> ResolveTarget(Entity actor, ActorAbility ability, ActorModifier modifier, Entity target,
            AbilityTargetting targetting, ConfigSelectTargets otherTargets = null)
        {
            ListComponent<Entity> res;
            switch (targetting)
            {
                case AbilityTargetting.Self:
                    res = ListComponent<Entity>.Create();
                    res.Add(actor);
                    break;
                case AbilityTargetting.Caster:
                    res = ListComponent<Entity>.Create();
                    res.Add(ability.Parent.GetParent<Entity>());
                    break;
                case AbilityTargetting.Target:
                    res = ListComponent<Entity>.Create();
                    res.Add(target);
                    break;
                case AbilityTargetting.SelfAttackTarget:
                    res = ListComponent<Entity>.Create();
                    var attackTarget = actor.GetComponent<CombatComponent>()?.GetAttackTarget();
                    if (attackTarget != null)
                    {
                        res.Add(attackTarget);
                    }
                    break;
                case AbilityTargetting.Applier:
                    res = ListComponent<Entity>.Create();
                    if (modifier != null)
                    {
                        var em = ability.Parent.GetParent<Entity>().Parent;
                        Entity applierEntity = em.Get<Entity>(modifier.ApplierID);
                        if (applierEntity != null)
                        {
                            res.Add(applierEntity);
                        }
                    }
                    else
                    {
                        res.Add(ability.Parent.GetParent<Entity>());
                    }
                    break;
                case AbilityTargetting.CurLocalAvatar:
                    res = ListComponent<Entity>.Create();
                    var scene = SceneManager.Instance.GetCurrentScene<MapScene>();
                    if (scene != null)
                    {
                        res.Add(scene.Self);
                    }
                    break;
                case AbilityTargetting.Other:
                    if (otherTargets != null)
                    {
                        res = otherTargets.ResolveTargets(actor, ability, modifier, target);
                    }
                    else
                    {
                        res = ListComponent<Entity>.Create();
                        Log.Error("指定AbilityTargetting.Other没有配置otherTargets");
                    }
                    break;
                case AbilityTargetting.Owner:
                    res = ListComponent<Entity>.Create();
                    var selfAc = actor.GetComponent<AttachComponent>();
                    if (selfAc == null || selfAc.ParentEntity == null)
                    {
                        res.Add(actor);
                    }
                    else
                    {
                        res.Add(selfAc.ParentEntity.GetParent<Entity>());
                    }
                    break;
                case AbilityTargetting.TargetOwner:
                    res = ListComponent<Entity>.Create();
                    var targetAc = target.GetComponent<AttachComponent>();
                    if (targetAc == null || targetAc.ParentEntity == null)
                    {
                        res.Add(target);
                    }
                    else
                    {
                        res.Add(targetAc.ParentEntity.GetParent<Entity>());
                    }
                    break;
                case AbilityTargetting.CasterOwner:
                    res = ListComponent<Entity>.Create();
                    var caster = ability.Parent.GetParent<Entity>();
                    var casterAc = caster.GetComponent<AttachComponent>();
                    if (casterAc == null || casterAc.ParentEntity == null)
                    {
                        res.Add(caster);
                    }
                    else
                    {
                        res.Add(casterAc.ParentEntity.GetParent<Entity>());
                    }
                    break;
                default:
                    res = ListComponent<Entity>.Create();
                    break;
            }
            return res;
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
                    return ability.Parent.GetParent<Entity>();
                case AttachPointTargetType.Owner:
                    var selfAc = actor.GetComponent<AttachComponent>();
                    if (selfAc == null || selfAc.ParentEntity == null)
                    {
                        return actor;
                    }
                    else
                    {
                        return selfAc.ParentEntity.GetParent<Entity>();
                    }
            }
            return null;
        }


        /// <summary>
        /// 获取目标
        /// </summary>
        /// <param name="attacker"></param>
        /// <param name="attackResult"></param>
        /// <param name="retreatType"></param>
        /// <returns></returns>
        public static Vector3 ResolveTarget(Entity attacker, AttackResult attackResult,Entity defence, RetreatType retreatType)
        {
            switch (retreatType)
            {
                case RetreatType.ByAttacker:
                    if (attacker is SceneEntity sceneEntity1 && defence is SceneEntity sceneEntity2)
                    {
                        return (sceneEntity2.Position - sceneEntity1.Position).normalized;
                    }
                    break;
                case RetreatType.ByTangent:
                    break;
                case RetreatType.ByHitDirection:
                    return attackResult.HitInfo.HitDir;
                case RetreatType.ByOriginOwner:
                    break;
                case RetreatType.ByHitDirectionInverse:
                    return -attackResult.HitInfo.HitDir;
            }
            return Vector3.zero;
        }
                
        public static bool IsTarget(Actor self, Actor other, TargetType type)
        {
            if (self == null || other == null) return false;
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