using Nino.Serialization;

namespace TaoTie
{
    /// <summary>
    /// 监听攻击后
    /// </summary>
    [NinoSerialize]
    public partial class ConfigDoActionAfterAttackMixin: ConfigAbilityMixin
    {
        [NinoMember(1)]
        public ConfigAbilityAction[] Actions;
        [NinoMember(2)]
        public ConfigAttackResult[] attackResultActions;

        public override AbilityMixin CreateAbilityMixin(ActorAbility actorAbility, ActorModifier actorModifier)
        {
            var res = ObjectPool.Instance.Fetch(TypeInfo<DoActionAfterAttackMixin>.Type) as DoActionAfterAttackMixin;
            res.Init(actorAbility, actorModifier, this);
            return res;
        }
    }
}