using Nino.Serialization;

namespace TaoTie
{
    /// <summary>
    /// 监听攻击后
    /// </summary>
    [NinoSerialize]
    public class ConfigAfterAttackMixin: ConfigAbilityMixin
    {
        [NinoMember(1)]
        public ConfigAbilityAction[] Actions;

        public override AbilityMixin CreateAbilityMixin(ActorAbility actorAbility, ActorModifier actorModifier)
        {
            var res = ObjectPool.Instance.Fetch(TypeInfo<AfterAttackMixin>.Type) as AfterAttackMixin;
            res.Init(actorAbility, actorModifier, this);
            return res;
        }
    }
}