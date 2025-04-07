using Nino.Core;

namespace TaoTie
{
    [NinoType(false)]
    public abstract class ConfigSimpleAttackPattern: ConfigBaseAttackPattern
    {
        [NotNull] [NinoMember(3)]
        public ConfigBornType Born;
    }
}