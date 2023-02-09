using System.Collections.Generic;

namespace TaoTie
{
    /// <summary>
    /// 监听状态机状态
    /// </summary>
    public class ConfigDoActionByStateIDMixin: ConfigAbilityMixin
    {
        public string ChargeLayer;
        public HashSet<string> StateIDs;
        public ConfigAbilityPredicate EnterPredicate;
        public ConfigAbilityAction[] EnterActions;
        public ConfigAbilityPredicate ExitPredicate;
        public ConfigAbilityAction[] ExitActions;
        public override AbilityMixin CreateAbilityMixin(ActorAbility actorAbility, ActorModifier actorModifier)
        {
            var res = ObjectPool.Instance.Fetch(TypeInfo<DoActionByStateIDMixin>.Type) as DoActionByStateIDMixin;
            res.Init(actorAbility, actorModifier, this);
            return res;
        }
    }
}