namespace TaoTie
{
    public class ConfigBeforeRemoveMixin : ConfigAbilityMixin
    {
        public ConfigAbilityAction[] Actions;

        public override AbilityMixin CreateAbilityMixin(ActorAbility actorAbility, ActorModifier actorModifier)
        {
            var res = ObjectPool.Instance.Fetch(TypeInfo<AfterAddMixin>.Type) as AfterAddMixin;
            res.Init(actorAbility, actorModifier, this);
            return res;
        }
    }
}