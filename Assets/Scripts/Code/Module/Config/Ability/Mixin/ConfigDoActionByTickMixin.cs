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
        [NinoMember(1)][LabelText("时间间隔(毫秒)")][MinValue(1)]
        public uint Interval = 1;
        
        [NinoMember(2)][LabelText("添加后立即tick一次")]
        public bool TickFirstOnAdd;
        [NinoMember(3)]
        public ConfigAbilityAction[] Actions;

        public override AbilityMixin CreateAbilityMixin(ActorAbility actorAbility, ActorModifier actorModifier)
        {
            var res = ObjectPool.Instance.Fetch<DoActionByTickMixin>();
            res.Init(actorAbility, actorModifier, this);
            return res;
        }
    }
}