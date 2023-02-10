using Nino.Serialization;

namespace TaoTie
{
    [NinoSerialize]
    public class ConfigAttackEvent
    {
        [NotNull][NinoMember(1)]
        public ConfigBaseAttackPattern AttackPattern;
        [NotNull][NinoMember(2)]
        public ConfigAttackInfo AttackInfo;
    }
}