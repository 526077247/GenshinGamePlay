using System.Collections.Generic;
using Nino.Core;
using Sirenix.OdinInspector;

namespace TaoTie
{
    /// <summary>
    /// 监听状态机状态
    /// </summary>
    [NinoType(false)][LabelText("监听状态机状态变化时AttachModify")]
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
            var res = ObjectPool.Instance.Fetch<AttachToStateIDMixin>();
            res.Init(actorAbility, actorModifier, this);
            return res;
        }
    }
}