using Nino.Serialization;
using Sirenix.OdinInspector;

namespace TaoTie
{
    [NinoSerialize]
    public partial class ConfigAbilityModifier
    {
        [NinoMember(1)]
        public string ModifierName;
        /// <summary>
        /// 持续时间，-1无限，0瞬时，0+毫秒
        /// </summary>
        [NinoMember(2)]
        public int Duration;
        [NinoMember(3)][ShowIf("@"+nameof(Duration)+"!=0")]
        public StackingType StackingType;
        [NinoMember(4)][ShowIf(nameof(StackingType),StackingType.Multiple)]
        public int StackLimitCount;
        [NinoMember(5)]
        public ConfigAbilityMixin[] Mixins;
        [NinoMember(6)][LabelText("修改玩家数值")]
        public ConfigCombatProperty[] Properties;
    }
}