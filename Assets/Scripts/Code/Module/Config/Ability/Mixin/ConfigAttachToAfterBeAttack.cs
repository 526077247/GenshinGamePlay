namespace TaoTie
{
    /// <summary>
    /// 受到攻击后
    /// </summary>
    public class ConfigAttachToAfterBeAttack: ConfigAbilityMixin
    {
        public ConfigAbilityAction[] Actions;
        public override AbilityMixin CreateAbilityMixin(ActorAbility actorAbility, ActorModifier actorModifier)
        {
            var res = ObjectPool.Instance.Fetch(TypeInfo<AttachToAfterBeAttack>.Type) as AttachToAfterBeAttack;
            res.Init(actorAbility, actorModifier, this);
            return res;
        }
    }
}