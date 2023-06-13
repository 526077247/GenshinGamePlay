using Nino.Serialization;
using Sirenix.OdinInspector;

namespace TaoTie
{
    /// <summary>
    /// 监听添加后
    /// </summary>
    [NinoSerialize][LabelText("ability或modify添加后DoAction")]
    public partial class ConfigDoActionAfterAddMixin : ConfigAbilityMixin
    {
        [NinoMember(1)]
        public ConfigAbilityAction[] Actions;

        public override AbilityMixin CreateAbilityMixin(ActorAbility actorAbility, ActorModifier actorModifier)
        {
            var res = ObjectPool.Instance.Fetch(TypeInfo<DoActionAfterAddMixin>.Type) as DoActionAfterAddMixin;
            res.Init(actorAbility, actorModifier, this);
            return res;
        }
    }
}