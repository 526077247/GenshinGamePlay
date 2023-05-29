using System.Collections.Generic;
using Nino.Serialization;

namespace TaoTie
{
    /// <summary>
    /// GadgetState状态改变
    /// </summary>
    [NinoSerialize]
    public partial class ConfigDoActionByGadgetStateMixin: ConfigAbilityMixin
    {
        [NinoMember(1)]
        public List<GadgetState> StateIDs;
        [NinoMember(2)]
        public ConfigAbilityPredicate EnterPredicate;
        [NinoMember(3)]
        public ConfigAbilityAction[] EnterActions;
        [NinoMember(4)]
        public ConfigAbilityPredicate ExitPredicate;
        [NinoMember(5)]
        public ConfigAbilityAction[] ExitActions;
        public override AbilityMixin CreateAbilityMixin(ActorAbility actorAbility, ActorModifier actorModifier)
        {
            var res = ObjectPool.Instance.Fetch(TypeInfo<DoActionByGadgetStateMixin>.Type) as DoActionByGadgetStateMixin;
            res.Init(actorAbility, actorModifier, this);
            return res;
        }
    }
}