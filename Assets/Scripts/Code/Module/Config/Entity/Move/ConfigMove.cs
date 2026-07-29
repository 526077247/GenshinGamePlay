using ProtoBuf;
using Sirenix.OdinInspector;

namespace TaoTie
{
    [ProtoContract]
    public partial class ConfigMove
    {
        [ProtoMember(1, IsRequired = true)][NotNull][LabelText("移动驱动方式")]
        public ConfigMoveAgent Agent = new ConfigAnimatorMove();
        [ProtoMember(2)][LabelText("初始控制逻辑")]
        public ConfigMoveStrategy DefaultStrategy;
    }
}