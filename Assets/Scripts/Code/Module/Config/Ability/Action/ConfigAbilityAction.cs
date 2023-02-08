namespace TaoTie
{
    public abstract class ConfigAbilityAction
    {
        public AbilityTargetting Targetting;
        public ConfigSelectTargets OtherTargets;
        protected abstract void Execute(Entity applier, ActorAbility ability, ActorModifier modifier, Entity target);

        public void DoExecute(Entity applier, ActorAbility ability, ActorModifier modifier, Entity target)
        {
            Entity[] targetLs = AbilityHelper.ResolveTarget(applier, ability, modifier, target,Targetting, OtherTargets);
            if (targetLs != null && targetLs.Length > 0)
            {
                foreach (Entity item in targetLs)
                {
                    if (item != null)
                    {
                        Execute(applier, ability, modifier, item);
                    }
                }
            }
            else
            {
                Log.Error("[ConfigAbilityAction:DoExecute] resolve action target is illegal,please check logic!");
            }
        }
    }
}