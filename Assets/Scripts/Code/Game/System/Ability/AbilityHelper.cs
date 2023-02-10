using System;

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
        public static Entity[] ResolveTarget(Entity actor, ActorAbility ability, ActorModifier modifier, Entity target,
            AbilityTargetting targetting, ConfigSelectTargets otherTargets = null)
        {
            switch (targetting)
            {
                case AbilityTargetting.Self:
                    return new[] { actor };
                case AbilityTargetting.Caster:
                    return new[] { ability.Parent.GetParent<Entity>() };
                case AbilityTargetting.Target:
                    return new[] { target };
                case AbilityTargetting.SelfAttackTarget:
                {
                    return null;
                }
                case AbilityTargetting.Applier:
                {
                    if (modifier != null)
                    {
                        var em = ability.Parent.GetParent<Entity>().Parent;
                        Entity applierEntity = em.Get<Entity>(modifier.ApplierID);
                        if (applierEntity != null)
                        {
                            return new[] { applierEntity };
                        }
                    }

                    return null;
                }
                case AbilityTargetting.CurLocalAvatar:
                {
                    var em = ability.Parent.GetParent<Entity>().Parent;
                    //返回当前(前台)角色
                    return null;
                }
                case AbilityTargetting.Other:
                    if (otherTargets == null) return null;
                    return otherTargets.ResolveTargets(actor, ability, modifier, target);
                default:
                    return null;
            }
        }

    }
}