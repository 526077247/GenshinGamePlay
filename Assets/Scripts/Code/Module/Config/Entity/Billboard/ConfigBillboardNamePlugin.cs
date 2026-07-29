using ProtoBuf;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [ProtoContract]
    public class ConfigBillboardNamePlugin: ConfigBillboardPlugin
    {
        [ProtoMember(10)][LabelText("是否展示Unit表对应名称")]
        public bool ShowUnitName;
        [ProtoMember(11)] [ShowIf("@!"+nameof(ShowUnitName))]
        public I18NKey NameI18NKey;
        [ProtoMember(12, IsRequired = true)]
        public Color BaseColor = Color.white;
    }
}