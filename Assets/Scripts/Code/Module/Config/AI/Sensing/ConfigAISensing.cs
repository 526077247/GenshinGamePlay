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
        public bool enable;
        public ConfigAISensingSetting setting;
        public Dictionary<int, ConfigAISensingSetting> settings;

    }
}