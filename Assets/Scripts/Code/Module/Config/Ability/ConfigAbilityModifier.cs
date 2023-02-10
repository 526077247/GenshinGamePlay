using Nino.Serialization;

namespace TaoTie
{
    public enum StackingType
    {
        Unique, // 只能存在唯一一个
        Multiple, // 互相独立存在
        Refresh, // 刷新已存在的modifier
        Prolong, // 延长已存在的modifier
    }
    [NinoSerialize]
    public class ConfigAbilityModifier
    {
        [NinoMember(1)]
        public string ModifierName;
        /// <summary>
        /// 持续时间，-1无限，0瞬时，0+毫秒
        /// </summary>
        [NinoMember(2)]
        public int Duration;
        [NinoMember(3)]
        public StackingType StackingType;
        [NinoMember(4)]
        public int StackLimitCount;
        [NinoMember(5)]
        public ConfigAbilityMixin[] Mixins;
    }
}