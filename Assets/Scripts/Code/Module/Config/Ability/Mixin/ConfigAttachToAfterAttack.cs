using Nino.Serialization;

namespace TaoTie
{
    /// <summary>
    /// 造成伤害后
    /// </summary>
    [NinoSerialize]
    public class ConfigAttachToAfterAttack: ConfigAbilityMixin
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