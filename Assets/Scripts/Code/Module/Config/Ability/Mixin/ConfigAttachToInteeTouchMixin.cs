using Nino.Serialization;

namespace TaoTie
{
    /// <summary>
    /// 监听点击面板按钮
    /// </summary>
    [NinoSerialize]
    public partial class ConfigAttachToInteeTouchMixin : ConfigAbilityMixin
    {
        [NinoMember(1)]
        public ConfigAbilityAction[] Actions;

        [NinoMember(2)]
        public int LocalId;

        public override AbilityMixin CreateAbilityMixin(ActorAbility actorAbility, ActorModifier actorModifier)
        {
            var res = ObjectPool.Instance.Fetch(TypeInfo<AttachToInteeTouchMixin>.Type) as AttachToInteeTouchMixin;
            res.Init(actorAbility, actorModifier, this);
            return res;
        }
    }
}