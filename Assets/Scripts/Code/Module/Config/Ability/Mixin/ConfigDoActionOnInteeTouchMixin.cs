using Nino.Serialization;
using Sirenix.OdinInspector;

namespace TaoTie
{
    /// <summary>
    /// 监听点击面板按钮
    /// </summary>
    [NinoSerialize][LabelText("点击面板按钮DoAction")]
    public partial class ConfigDoActionOnInteeTouchMixin : ConfigAbilityMixin
    {
        [NinoMember(1)]
        public ConfigAbilityAction[] Actions;

        [NinoMember(2)]
        public int LocalId;

        public override AbilityMixin CreateAbilityMixin(ActorAbility actorAbility, ActorModifier actorModifier)
        {
            var res = ObjectPool.Instance.Fetch(TypeInfo<DoActionOnInteeTouchMixin>.Type) as DoActionOnInteeTouchMixin;
            res.Init(actorAbility, actorModifier, this);
            return res;
        }
    }
}