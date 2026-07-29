using System.Collections.Generic;
using ProtoBuf;
using Sirenix.OdinInspector;

namespace TaoTie
{
    [ProtoContract]
    public partial class ConfigAISensing
    {
        [LabelText("启用")]
        [ProtoMember(1, IsRequired = true)]
        public bool Enable = true;
        [ProtoMember(2)]
        public ConfigAISensingSetting Setting;
        [ProtoMember(3, IsRequired = true)]
        public Dictionary<string, ConfigAISensingSetting> Settings = new Dictionary<string, ConfigAISensingSetting>();

    }
}