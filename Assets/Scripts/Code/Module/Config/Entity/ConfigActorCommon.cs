using ProtoBuf;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using TaoTie.Inspector;
#endif
using UnityEngine;

namespace TaoTie
{
    [ProtoContract]
    public partial class ConfigActorCommon
    {
        [ProtoMember(1, IsRequired = true)][LabelText("*高度")][Tooltip("会影响相机机位")][MinValue(0.01f)]
        public float Height = 1.5f;
        [ProtoMember(3, IsRequired = true)][LabelText("预制体缩放")]
        public float Scale = 1;
        [ProtoMember(2, IsRequired = true)][LabelText("*模型高度")][Tooltip("影响寻路和避障")][MinValue(0.01f)]
        public float ModelHeight = 1.5f;
        [ProtoMember(5, IsRequired = true)][LabelText("*模型半径")][Tooltip("影响寻路和避障")][MinValue(0.01f)]
        public float ModelRadius = 0.7f;
        [ProtoMember(4, IsRequired = true)][LabelText("相机近景模式偏移高度")]
        public float NearFocusOffsetHeight = 0.5f;
    }
}