using Nino.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [NinoType(false)][LabelText("寻路")]
    public partial class ConfigPlatformMove: ConfigMoveStrategy
    {
        [NinoMember(10)]
        public ConfigRoute Route;
        [NinoMember(11)][LabelText("*延迟启动(ms)")][Tooltip("<0不启动；0立刻；>0延迟多久")][ShowIf("@"+nameof(Route)+"!=null")]
        public int Delay = -1;
    }
}