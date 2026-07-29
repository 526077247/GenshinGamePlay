using TaoTie.LitJson.Extensions;
using ProtoBuf;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace TaoTie
{
    [ProtoContract]
    [ProtoInclude(102, typeof(ConfigBillboardPrefabPlugin))]
    [ProtoInclude(100, typeof(ConfigBillboardHpPlugin))]
    [ProtoInclude(101, typeof(ConfigBillboardNamePlugin))]
    public abstract partial class ConfigBillboardPlugin
    {
        [ProtoMember(1)]
        public Vector3 Offset;
    }
}