namespace TaoTie
{
    /// <summary>
    /// 按条件过滤
    /// </summary>
    public class Predicated: ConfigAbilityAction
    {
        public ConfigAbilityPredicate TargetPredicate;
        public ConfigAbilityAction[] SuccessActions;
        public ConfigAbilityAction[] FailActions;
        
        protected override void Execute(Entity applier, ActorAbility ability, ActorModifier modifier, Entity target)
        {
            if (TargetPredicate.Evaluate(applier, ability, modifier, target))
            {
                if (SuccessActions != null)
                {
                    for (int i = 0; i < SuccessActions.Length; i++)
                    {
                        SuccessActions[i].DoExecute(applier, ability, modifier, target);
                    }
                }
            }
            else
            {
                if (FailActions != null)
                {
                    for (int i = 0; i < FailActions.Length; i++)
                    {
                        FailActions[i].DoExecute(applier, ability, modifier, target);
                    }
                }
            }
        }
    }
}