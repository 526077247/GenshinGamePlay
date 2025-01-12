using Nino.Core;
using UnityEngine;

namespace TaoTie
{
    [NinoType(false)]
    public partial class ConfigFsmTimeline
    {
        [NinoMember(1)]
        public ConfigFsmClip[] Clips;
    }
}