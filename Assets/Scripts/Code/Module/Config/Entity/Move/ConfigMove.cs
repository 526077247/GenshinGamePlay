using Nino.Core;

namespace TaoTie
{
    [NinoType(false)]
    public class ConfigMove
    {
        [NinoMember(1)][NotNull]
        public ConfigMoveStrategy Strategy = new ConfigAnimatorMove();
        [NinoMember(2)]
        public ConfigMoveAgent DefaultAgent;
    }
}