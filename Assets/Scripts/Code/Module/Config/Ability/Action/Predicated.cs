using ProtoBuf;

namespace TaoTie
{
    /// <summary>
    /// 按条件过滤
    /// </summary>
    [ProtoContract]
    public partial class Predicated: ConfigAbilityAction
    {
        [ProtoMember(10)]
        public ConfigAbilityPredicate TargetPredicate;
        [ProtoMember(11)]
        public ConfigAbilityAction[] SuccessActions;
        [ProtoMember(12)]
        public ConfigAbilityAction[] FailActions;
        
        protected override void Execute(Entity actionExecuter, ActorAbility ability, ActorModifier modifier, Entity target)
        {
            if (TargetPredicate.Evaluate(actionExecuter, ability, modifier, target))
            {
                if (SuccessActions != null)
                {
                    for (int i = 0; i < SuccessActions.Length; i++)
                    {
                        SuccessActions[i].DoExecute(actionExecuter, ability, modifier, target);
                    }
                }
            }
            else
            {
                if (FailActions != null)
                {
                    for (int i = 0; i < FailActions.Length; i++)
                    {
                        FailActions[i].DoExecute(actionExecuter, ability, modifier, target);
                    }
                }
            }
        }
    }
}