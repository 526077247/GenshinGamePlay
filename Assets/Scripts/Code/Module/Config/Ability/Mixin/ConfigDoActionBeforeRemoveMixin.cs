using Nino.Serialization;
using Sirenix.OdinInspector;

namespace TaoTie
{
    /// <summary>
    /// 监听移除前
    /// </summary>
    [NinoSerialize][LabelText("ability或modify移除前DoAction")]
    public partial class ConfigDoActionBeforeRemoveMixin : ConfigAbilityMixin
    {
        [NinoMember(1)]
        public ConfigAbilityAction[] Actions;

        public override AbilityMixin CreateAbilityMixin(ActorAbility actorAbility, ActorModifier actorModifier)
        {
            var res = ObjectPool.Instance.Fetch(TypeInfo<DoActionBeforeRemoveMixin>.Type) as DoActionBeforeRemoveMixin;
            res.Init(actorAbility, actorModifier, this);
            return res;
        }
    }
}