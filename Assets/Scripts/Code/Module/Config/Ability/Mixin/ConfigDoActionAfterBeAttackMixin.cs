using Nino.Core;
using Sirenix.OdinInspector;

namespace TaoTie
{
    /// <summary>
    /// 受到攻击后
    /// </summary>
    [NinoType(false)][LabelText("当受到攻击后DoAction")]
    public partial class ConfigDoActionAfterBeAttackMixin: ConfigAbilityMixin
    {
        [NinoMember(1)]
        public ConfigAbilityAction[] Actions;
        public override AbilityMixin CreateAbilityMixin(ActorAbility actorAbility, ActorModifier actorModifier)
        {
            var res = ObjectPool.Instance.Fetch(TypeInfo<DoActionAfterBeAttackMixin>.Type) as DoActionAfterBeAttackMixin;
            res.Init(actorAbility, actorModifier, this);
            return res;
        }
    }
}