using System.Collections.Generic;
using ProtoBuf;

namespace TaoTie
{
    [ProtoContract]
    public partial class ConfigEquipController
    {
        [ProtoMember(1, IsRequired = true)]
        public Dictionary<EquipType, string> AttachPoints = new Dictionary<EquipType, string>();
        [ProtoMember(2)]
        public string SheathPoint;
    }
}