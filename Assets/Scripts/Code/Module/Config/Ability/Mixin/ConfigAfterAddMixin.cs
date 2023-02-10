using Nino.Serialization;

namespace TaoTie
{
    /// <summary>
    /// 监听添加后
    /// </summary>
    [NinoSerialize]
    public class ConfigAfterAddMixin : ConfigAbilityMixin
    {
        [NinoMember(1)]
        public ConfigAbilityAction[] Actions;

        public override AbilityMixin CreateAbilityMixin(ActorAbility actorAbility, ActorModifier actorModifier)
        {
            var res = ObjectPool.Instance.Fetch(TypeInfo<AfterAddMixin>.Type) as AfterAddMixin;
            res.Init(actorAbility, actorModifier, this);
            return res;
        }
    }
}