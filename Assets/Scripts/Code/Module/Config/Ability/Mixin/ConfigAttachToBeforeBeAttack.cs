namespace TaoTie
{
    /// <summary>
    /// 受到攻击前
    /// </summary>
    public class ConfigAttachToBeforeBeAttack: ConfigAbilityMixin
    {
        public ConfigAbilityAction[] Actions;
        public override AbilityMixin CreateAbilityMixin(ActorAbility actorAbility, ActorModifier actorModifier)
        {
            var res = ObjectPool.Instance.Fetch(TypeInfo<AttachToBeforeBeAttack>.Type) as AttachToBeforeBeAttack;
            res.Init(actorAbility, actorModifier, this);
            return res;
        }
    }
}