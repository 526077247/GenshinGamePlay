using Nino.Core;

namespace TaoTie
{
    [NinoType(false)]
    public partial class ConfigAttackEvent
    {
        [NotNull][NinoMember(1)]
        public ConfigBaseAttackPattern AttackPattern;
        [NotNull][NinoMember(2)]
        public ConfigAttackInfo AttackInfo = new ConfigAttackInfo();
    }
}