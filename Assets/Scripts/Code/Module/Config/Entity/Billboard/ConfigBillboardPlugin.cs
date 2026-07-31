using TaoTie.LitJson.Extensions;
using ProtoBuf;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using TaoTie.Inspector;
#endif
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