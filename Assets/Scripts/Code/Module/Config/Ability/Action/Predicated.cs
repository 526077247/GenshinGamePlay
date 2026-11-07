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
            Execute(actionExecuter, ability, modifier, target, null);
        }

        /// <summary>
        /// 转发 result：包在 Predicated 里的 <see cref="ModifyAttackResult"/> 也要能拿到本次结算，
        /// 否则"条件成立才增伤"这种写法会静默失效。
        /// </summary>
        protected override void Execute(Entity actionExecuter, ActorAbility ability, ActorModifier modifier,
            Entity target, AttackResult result)
        {
            if (TargetPredicate.Evaluate(actionExecuter, ability, modifier, target))
            {
                if (SuccessActions != null)
                {
                    for (int i = 0; i < SuccessActions.Length; i++)
                    {
                        SuccessActions[i].DoExecute(actionExecuter, ability, modifier, target, result);
                    }
                }
            }
            else
            {
                if (FailActions != null)
                {
                    for (int i = 0; i < FailActions.Length; i++)
                    {
                        FailActions[i].DoExecute(actionExecuter, ability, modifier, target, result);
                    }
                }
            }
        }
    }
}