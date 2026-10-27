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
        /// 解析「攻击起点」实体。
        ///
        /// 子弹命中时攻击者可能被解析成子弹自己（TriggerAttackEvent 直接拿 actionExecuter 当攻击者），
        /// 而子弹此刻就贴在受击者身上，拿它算方向必然退化成零向量。所以起点优先取射手
        /// （<see cref="AttackResult.OriginOwnerId"/>，由 CreateBullet 记录）。
        /// 取不到（OriginOwnerId=0 / 射手已销毁）时退回攻击者本人，等价于旧行为 ——
        /// 配错类型的攻击不会变成「打不退」。
        /// </summary>
        public static Entity ResolveAttackOrigin(Entity attacker, Entity defence, AttackResult attackResult)
        {
            if (attackResult != null && attackResult.OriginOwnerId != 0)
            {
                var em = attacker != null ? attacker.Parent : defence?.Parent;
                var owner = em != null ? em.Get<Entity>(attackResult.OriginOwnerId) : null;
                if (owner != null) return owner;
            }
            return attacker;
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
                case RetreatType.ByOriginOwner:
                {
                    var origin = retreatType == RetreatType.ByOriginOwner
                        ? ResolveAttackOrigin(attacker, defence, attackResult)
                        : attacker;
                    if (origin is SceneEntity from && defence is SceneEntity to)
                    {
                        var dir = to.Position - from.Position;
                        dir.y = 0f; // 水平推：竖直分量由 HitImpulseY 单独控制
                        if (dir.sqrMagnitude > 1e-6f) return dir.normalized;
                    }
                    break;
                }
                case RetreatType.ByTangent:
                {
                    // 切向 = 绕 Y 轴把「攻击者 → 受击者」连线转 +90°（左手侧）。方向必须是确定的：
                    // 若按命中点做叉乘，同一招打同一位置会因为命中点微差左右乱跳。
                    Vector3 baseDir = default;
                    if (attacker is SceneEntity a && defence is SceneEntity d)
                        baseDir = d.Position - a.Position;
                    if (baseDir.sqrMagnitude < 1e-6f)
                        baseDir = attackResult.HitInfo.HitDir; // 双方重合（叠在同一个点上）时退回命中方向
                    baseDir.y = 0f;
                    if (baseDir.sqrMagnitude < 1e-6f) break;

                    // Cross(dir, up) 得到水平左手侧；叉积顺序不能换，换了就是另一侧
                    return Vector3.Cross(baseDir.normalized, Vector3.up);
                }
                case RetreatType.ByHitDirection:
                    return attackResult.HitInfo.HitDir;
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