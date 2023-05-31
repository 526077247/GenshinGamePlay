using Nino.Serialization;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [NinoSerialize]
    public class ConfigBillboardNamePlugin: ConfigBillboardPlugin
    {
        [NinoMember(10)][LabelText("是否展示Unit表对应名称")]
        public bool ShowUnitName;
        [NinoMember(11)] [ShowIf("@!"+nameof(ShowUnitName))]
        public string NameI18NKey;
        [NinoMember(12)]
        public Color BaseColor = Color.white;
    }
}