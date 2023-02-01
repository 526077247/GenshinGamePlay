namespace TaoTie
{
    public class ConfigTickMixin:ConfigAbilityMixin
    {
        /// <summary>
        /// 时间间隔毫秒
        /// </summary>
        public uint Interval;
        /// <summary>
        /// 添加后立即tick一次
        /// </summary>
        public bool TickFirstOnAdd;
        public ConfigAbilityAction[] Actions;
        
        public override AbilityMixin CreateAbilityMixin(ActorAbility actorAbility)
        {
            var res = ObjectPool.Instance.Fetch(typeof(TickMixin)) as TickMixin;
            res.Init(actorAbility,this);
            return res;
        }
    }
}