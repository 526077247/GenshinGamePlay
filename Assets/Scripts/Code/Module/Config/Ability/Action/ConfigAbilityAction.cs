namespace TaoTie
{
    public enum AbilityActionTarget
    {
        Self,
        Caster,
        Target,
        SelfAttackTarget,
        Applier,    // modifier applier
        CurLocalAvatar,
    }
    public abstract class ConfigAbilityAction
    {
        public AbilityActionTarget ActionTarget;

        protected abstract void Execute(Entity applier, ActorAbility ability, ActorModifier modifier, Entity aim);

        public void DoExecute(Entity applier, ActorAbility ability, ActorModifier modifier, Entity aim)
        {
            Entity[] targetLs = ResolveActionTarget(applier, ability, modifier, aim);
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
        
        private Entity[] ResolveActionTarget(Entity actor, ActorAbility ability, ActorModifier modifier, Entity other)
        {
            switch (ActionTarget)
            {
                case AbilityActionTarget.Self:
                    return new[] { actor };
                case AbilityActionTarget.Caster:
                    return new[] { ability.Parent.GetParent<Entity>() };
                case AbilityActionTarget.Target:
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
                    var em = ability.Parent.GetParent<Entity>().Parent;
                    //返回当前(前台)角色
                    return null;
                }
                default:
                    return null;
            }
        }
    }
}