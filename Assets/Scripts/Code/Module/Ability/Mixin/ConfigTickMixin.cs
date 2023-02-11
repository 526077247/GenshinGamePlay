using Nino.Serialization;

namespace TaoTie
{
    /// <summary>
    /// 监听间隔
    /// </summary>
    [NinoSerialize]
    public partial class ConfigTickMixin : ConfigAbilityMixin
    {
        /// <summary>
        /// 时间间隔毫秒
        /// </summary>
        [NinoMember(1)]
        public uint Interval;

        /// <summary>
        /// 添加后立即tick一次
        /// </summary>
        [NinoMember(2)]
        public bool TickFirstOnAdd;
        [NinoMember(3)]
        public ConfigAbilityAction[] Actions;

        public override AbilityMixin CreateAbilityMixin(ActorAbility actorAbility, ActorModifier actorModifier)
        {
            var res = ObjectPool.Instance.Fetch(TypeInfo<TickMixin>.Type) as TickMixin;
            res.Init(actorAbility, actorModifier, this);
            return res;
        }
    }
}