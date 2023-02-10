namespace TaoTie
{
    /// <summary>
    /// 造成伤害后
    /// </summary>
    public class ConfigAttachToAfterAttack: ConfigAbilityMixin
    {
        public ConfigAbilityAction[] Actions;
        public override AbilityMixin CreateAbilityMixin(ActorAbility actorAbility, ActorModifier actorModifier)
        {
            var res = ObjectPool.Instance.Fetch(TypeInfo<AfterAttackMixin>.Type) as AfterAttackMixin;
            res.Init(actorAbility, actorModifier, this);
            return res;
        }
    }
}