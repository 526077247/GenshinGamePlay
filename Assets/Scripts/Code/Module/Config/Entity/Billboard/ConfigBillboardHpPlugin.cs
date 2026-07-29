using ProtoBuf;
using UnityEngine;

namespace TaoTie
{
    
    [ProtoContract]
    public class ConfigBillboardHpPlugin: ConfigBillboardPlugin
    {
        [ProtoMember(10, IsRequired = true)]
        public Color BleedColor = Color.green;
        [ProtoMember(11, IsRequired = true)]
        public Color BgColor = Color.grey;
        [ProtoMember(12, IsRequired = true)]
        public Vector2 Size = new Vector2(200, 10);
    }
}