namespace TaoTie
{
    public class ConfigAfterAttackMixin: ConfigAbilityMixin
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