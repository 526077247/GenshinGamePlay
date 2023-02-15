using System.Collections.Generic;
using Nino.Serialization;
using Sirenix.OdinInspector;

namespace TaoTie
{
    [NinoSerialize]
    public partial class ConfigAISensing
    {
        [LabelText("启用")]
        public bool enable;
        public ConfigAISensingSetting defaultSetting;
        public Dictionary<int, ConfigAISensingSetting> settings;

    }
}