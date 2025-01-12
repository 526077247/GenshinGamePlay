using Nino.Core;
using Sirenix.OdinInspector;

namespace TaoTie
{
    /// <summary>
    /// 监听间隔
    /// </summary>
    [NinoType(false)][LabelText("间隔时间DoAction")]
    public partial class ConfigDoActionByTickMixin : ConfigAbilityMixin
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
            var res = ObjectPool.Instance.Fetch(TypeInfo<DoActionByTickMixin>.Type) as DoActionByTickMixin;
            res.Init(actorAbility, actorModifier, this);
            return res;
        }
    }
}