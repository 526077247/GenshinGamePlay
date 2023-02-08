namespace TaoTie
{
    /// <summary>
    /// 监听调用Execute方法
    /// </summary>
    public class ConfigExecuteMixin : ConfigAbilityMixin
    {
        public ConfigAbilityAction[] Actions;

        public override AbilityMixin CreateAbilityMixin(ActorAbility actorAbility, ActorModifier actorModifier)
        {
            var res = ObjectPool.Instance.Fetch(TypeInfo<ExecuteMixin>.Type) as ExecuteMixin;
            res.Init(actorAbility, actorModifier, this);
            return res;
        }
    }
}