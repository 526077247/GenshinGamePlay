namespace TaoTie
{
    public class ConfigAfterAddMixin:ConfigAbilityMixin
    {
        public ConfigAbilityAction[] Actions;

        public override AbilityMixin CreateAbilityMixin(Ability ability)
        {
            var res = ObjectPool.Instance.Fetch(typeof(AfterAddMixin)) as AfterAddMixin;
            res.Init(ability,this);
            return res;
        }
    }
}