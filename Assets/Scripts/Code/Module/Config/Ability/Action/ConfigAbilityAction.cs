namespace TaoTie
{
    public enum AbilityActionTarget
    {
        Self,
        Caster,
        Enemy,
        SelfAttackTarget,
        Applier,    // modifier applier
        CurTeamAvatars,
        CurLocalAvatar,
        Team,
        Owner,
    }
    public abstract class ConfigAbilityAction
    {
        public AbilityActionTarget ActionTarget;

        protected abstract void Execute(Entity applier, ActorAbility ability, ActorModifier modifier, Entity other);

        public virtual void DoExecute(Entity applier, ActorAbility ability, ActorModifier modifier, Entity other)
        {
            Entity[] targetLs = ResolveActionTarget(ActionTarget, applier, ability, modifier, other);
            if (targetLs != null && targetLs.Length > 0)
            {
                foreach (Entity target in targetLs)
                {
                    if (target != null)
                    {
                        Execute(applier, ability, modifier, target);
                    }
                }
            }
            else
            {
                Log.Error("[ConfigAbilityAction:DoExecute] resolve action target is illegal,please check logic!");
            }
        }
        
        private Entity[] ResolveActionTarget(AbilityActionTarget actionTarget, Entity actor, ActorAbility ability, ActorModifier modifier, Entity other)
        {
            switch (actionTarget)
            {
                case AbilityActionTarget.Self:
                    return new[] { actor };
                case AbilityActionTarget.Caster:
                    return new[] { ability.Parent.GetParent<Entity>() };
                case AbilityActionTarget.Enemy:
                    return new[] { other };
                case AbilityActionTarget.SelfAttackTarget:
                {
                    return null;
                }
                case AbilityActionTarget.Applier:
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
                case AbilityActionTarget.CurLocalAvatar:
                {
                    //返回当前(前台)角色
                    return null;
                }
                case AbilityActionTarget.Team:
                {
                    return null;
                }
                case AbilityActionTarget.CurTeamAvatars:
                {
                    return null;
                }
                case AbilityActionTarget.Owner:
                {
                    return null;
                }
                default:
                    return null;
            }
        }
    }
}