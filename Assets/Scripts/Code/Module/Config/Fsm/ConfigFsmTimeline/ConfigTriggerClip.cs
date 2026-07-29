using ProtoBuf;
using Sirenix.OdinInspector;

namespace TaoTie
{
    [ProtoContract]
    public partial class ConfigTriggerClip:ConfigFsmClip
    {
        [ProtoMember(10)]
        public string TriggerId;
        [ProtoMember(11)][LabelText("当还未开始时被打断是否触发")]
        public bool TriggerOnBreak;
    }
}