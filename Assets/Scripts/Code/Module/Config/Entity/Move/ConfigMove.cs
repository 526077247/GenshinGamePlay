using Nino.Core;
using Sirenix.OdinInspector;

namespace TaoTie
{
    [NinoType(false)]
    public partial class ConfigMove
    {
        [NinoMember(1)][NotNull][LabelText("移动驱动方式")]
        public ConfigMoveAgent Agent = new ConfigAnimatorMove();
        [NinoMember(2)][LabelText("初始控制逻辑")]
        public ConfigMoveStrategy DefaultStrategy;
    }
}