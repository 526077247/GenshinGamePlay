using Nino.Core;
using Sirenix.OdinInspector;

namespace TaoTie
{
    [NinoType(false)]
    public partial class ConfigTriggerClip:ConfigFsmClip
    {
        [NinoMember(10)]
        public string TriggerId;
        [NinoMember(11)][LabelText("当还未开始时被打断是否触发")]
        public bool TriggerOnBreak;
    }
}