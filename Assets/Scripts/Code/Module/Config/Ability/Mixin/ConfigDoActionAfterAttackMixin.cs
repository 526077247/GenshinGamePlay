using Nino.Serialization;
using Sirenix.OdinInspector;

namespace TaoTie
{
    /// <summary>
    /// 监听攻击后
    /// </summary>
    [NinoSerialize][LabelText("攻击后DoAction")]
    public partial class ConfigDoActionAfterAttackMixin: ConfigAbilityMixin
    {
        [NinoMember(1)]
        public ConfigAbilityAction[] Actions;

        public override AbilityMixin CreateAbilityMixin(ActorAbility actorAbility, ActorModifier actorModifier)
        {
            var res = ObjectPool.Instance.Fetch(TypeInfo<DoActionAfterAttackMixin>.Type) as DoActionAfterAttackMixin;
            res.Init(actorAbility, actorModifier, this);
            return res;
        }
    }
}