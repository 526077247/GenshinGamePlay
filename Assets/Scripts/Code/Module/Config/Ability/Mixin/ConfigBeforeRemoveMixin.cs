using Nino.Serialization;

namespace TaoTie
{
    /// <summary>
    /// 监听移除前
    /// </summary>
    [NinoSerialize]
    public partial class ConfigBeforeRemoveMixin : ConfigAbilityMixin
    {
        [NinoMember(1)]
        public ConfigAbilityAction[] Actions;

        public override AbilityMixin CreateAbilityMixin(ActorAbility actorAbility, ActorModifier actorModifier)
        {
            var res = ObjectPool.Instance.Fetch(TypeInfo<BeforeRemoveMixin>.Type) as BeforeRemoveMixin;
            res.Init(actorAbility, actorModifier, this);
            return res;
        }
    }
}