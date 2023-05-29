using System.Collections.Generic;
using Nino.Serialization;

namespace TaoTie
{
    /// <summary>
    /// 监听状态机状态
    /// </summary>
    [NinoSerialize]
    public partial class ConfigAttachToStateIDMixin: ConfigAbilityMixin
    {
        [NinoMember(1)]
        public string ChargeLayer;
        [NinoMember(2)]
        public List<string> StateIDs;
        [NinoMember(3)]
        public ConfigAbilityPredicate Predicate;
        [NinoMember(4)]
        public string ModifierName;
        public override AbilityMixin CreateAbilityMixin(ActorAbility actorAbility, ActorModifier actorModifier)
        {
            var res = ObjectPool.Instance.Fetch(TypeInfo<AttachToStateIDMixin>.Type) as AttachToStateIDMixin;
            res.Init(actorAbility, actorModifier, this);
            return res;
        }
    }
}