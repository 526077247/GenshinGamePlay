namespace TaoTie
{
    public enum StackingType
    {
        Unique, // 只能存在唯一一个
        Multiple, // 互相独立存在
        Refresh, // 刷新已存在的modifier
        Prolong, // 延长已存在的modifier
    }
    public abstract class ConfigAbilityModifier
    {
        public string ModifierName;
        /// <summary>
        /// 持续时间，-1无限，0瞬时，0+毫秒
        /// </summary>
        public int Duration;
        public StackingType StackingType;
        public ConfigAbilityMixin[] Mixins;
    }
}