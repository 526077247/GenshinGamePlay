using Nino.Serialization;

namespace TaoTie
{
    /// <summary>
    /// 受到攻击后
    /// </summary>
    [NinoSerialize]
    public class ConfigAttachToAfterBeAttack: ConfigAbilityMixin
    {
        [NinoMember(1)]
        public ConfigAbilityAction[] Actions;
        public override AbilityMixin CreateAbilityMixin(ActorAbility actorAbility, ActorModifier actorModifier)
        {
            var res = ObjectPool.Instance.Fetch(TypeInfo<AttachToAfterBeAttack>.Type) as AttachToAfterBeAttack;
            res.Init(actorAbility, actorModifier, this);
            return res;
        }
    }
}