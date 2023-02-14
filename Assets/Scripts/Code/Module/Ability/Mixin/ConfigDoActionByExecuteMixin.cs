using Nino.Serialization;

namespace TaoTie
{
    /// <summary>
    /// 监听调用Execute方法
    /// </summary>
    [NinoSerialize]
    public partial class ConfigDoActionByExecuteMixin : ConfigAbilityMixin
    {
        [NinoMember(1)]
        public ConfigAbilityAction[] Actions;

        public override AbilityMixin CreateAbilityMixin(ActorAbility actorAbility, ActorModifier actorModifier)
        {
            var res = ObjectPool.Instance.Fetch(TypeInfo<DoActionByExecuteMixin>.Type) as DoActionByExecuteMixin;
            res.Init(actorAbility, actorModifier, this);
            return res;
        }
    }
}