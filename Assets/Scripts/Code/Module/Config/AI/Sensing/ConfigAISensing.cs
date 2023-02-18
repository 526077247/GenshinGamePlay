using System.Collections.Generic;
using Nino.Serialization;
using Sirenix.OdinInspector;

namespace TaoTie
{
    [NinoSerialize]
    public partial class ConfigAISensing
    {
        [LabelText("启用")]
        [NinoMember(1)]
        public bool Enable;
        [NinoMember(2)]
        public ConfigAISensingSetting Setting;
        [NinoMember(3)]
        public Dictionary<string, ConfigAISensingSetting> Settings;

    }
}