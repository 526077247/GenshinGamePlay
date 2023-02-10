using System.Collections.Generic;
using Nino.Serialization;

namespace TaoTie
{
    /// <summary>
    /// 监听状态机状态
    /// </summary>
    [NinoSerialize]
    public class ConfigDoActionByStateIDMixin: ConfigAbilityMixin
    {
        [NinoMember(1)]
        public string ChargeLayer;
        [NinoMember(2)]
        public HashSet<string> StateIDs;
        [NinoMember(3)]
        public ConfigAbilityPredicate EnterPredicate;
        [NinoMember(4)]
        public ConfigAbilityAction[] EnterActions;
        [NinoMember(5)]
        public ConfigAbilityPredicate ExitPredicate;
        [NinoMember(6)]
        public ConfigAbilityAction[] ExitActions;
        public override AbilityMixin CreateAbilityMixin(ActorAbility actorAbility, ActorModifier actorModifier)
        {
            var res = ObjectPool.Instance.Fetch(TypeInfo<DoActionByStateIDMixin>.Type) as DoActionByStateIDMixin;
            res.Init(actorAbility, actorModifier, this);
            return res;
        }
    }
}