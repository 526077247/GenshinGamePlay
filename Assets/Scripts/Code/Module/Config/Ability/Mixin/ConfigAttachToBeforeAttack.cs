namespace TaoTie
{
    /// <summary>
    /// 攻击前
    /// </summary>
    public class ConfigAttachToBeforeAttack: ConfigAbilityMixin
    {
        public ConfigAbilityAction[] Actions;
        public override AbilityMixin CreateAbilityMixin(ActorAbility actorAbility, ActorModifier actorModifier)
        {
            var res = ObjectPool.Instance.Fetch(TypeInfo<AttachToBeforeAttack>.Type) as AttachToBeforeAttack;
            res.Init(actorAbility, actorModifier, this);
            return res;
        }
    }
}