using Nino.Serialization;

namespace TaoTie
{
    /// <summary>
    /// 攻击前
    /// </summary>
    [NinoSerialize]
    public partial class ConfigDoActionBeforeAttackMixin: ConfigAbilityMixin
    {
        [NinoMember(1)]
        public ConfigAbilityAction[] Actions;
        public override AbilityMixin CreateAbilityMixin(ActorAbility actorAbility, ActorModifier actorModifier)
        {
            var res = ObjectPool.Instance.Fetch(TypeInfo<DoActionBeforeAttackMixin>.Type) as DoActionBeforeAttackMixin;
            res.Init(actorAbility, actorModifier, this);
            return res;
        }
    }
}