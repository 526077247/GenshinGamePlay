using Nino.Serialization;

namespace TaoTie
{
    /// <summary>
    /// 监听调用Execute方法
    /// </summary>
    [NinoSerialize]
    public class ConfigExecuteMixin : ConfigAbilityMixin
    {
        [NinoMember(1)]
        public ConfigAbilityAction[] Actions;

        public override AbilityMixin CreateAbilityMixin(ActorAbility actorAbility, ActorModifier actorModifier)
        {
            var res = ObjectPool.Instance.Fetch(TypeInfo<ExecuteMixin>.Type) as ExecuteMixin;
            res.Init(actorAbility, actorModifier, this);
            return res;
        }
    }
}