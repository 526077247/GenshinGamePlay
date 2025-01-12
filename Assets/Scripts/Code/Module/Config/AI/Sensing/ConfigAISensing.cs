using System.Collections.Generic;
using Nino.Core;
using Sirenix.OdinInspector;

namespace TaoTie
{
    [NinoType(false)]
    public partial class ConfigAISensing
    {
        [LabelText("启用")]
        [NinoMember(1)]
        public bool Enable = true;
        [NinoMember(2)]
        public ConfigAISensingSetting Setting;
        [NinoMember(3)]
        public Dictionary<string, ConfigAISensingSetting> Settings = new();

    }
}