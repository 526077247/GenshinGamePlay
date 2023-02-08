using System.Collections.Generic;

namespace TaoTie
{
    /// <summary>
    /// 监听状态机状态
    /// </summary>
    public class ConfigAttachToStateIDMixin: ConfigAbilityMixin
    {
        public string ChargeLayer;
        public HashSet<string> StateIDs;
        public ConfigAbilityPredicate Predicate;
        public string ModifierName;
        public override AbilityMixin CreateAbilityMixin(ActorAbility actorAbility, ActorModifier actorModifier)
        {
            var res = ObjectPool.Instance.Fetch(TypeInfo<AttachToStateIDMixin>.Type) as AttachToStateIDMixin;
            res.Init(actorAbility, actorModifier, this);
            return res;
        }
    }
}