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
        
        [ProtoMember(6, IsRequired = true)][LabelText("参与角色互斥")][Tooltip("false = 可被其他角色穿过（宝箱/子弹/装饰物）。只控制几何阻挡；挨打时的击退强度由 HitImpulseResists 决定")]
        public bool CanBlock = true;
        [ProtoMember(7)][LabelText("身体互斥半径")][Tooltip("BodyProxies 为空时使用的单圆半径。注意与 ModelRadius（墙扫掠膨胀半径）无关")][MinValue(0.01f)]
        public float BodyRadius = 0.3f;
        [ProtoMember(8)][LabelText("身体代理(可空)")][Tooltip("大型/多足模型可配置多个代理；为空则按 BodyRadius 生成单圆。代理偏移在模型空间，随 yaw 旋转")]
        public ConfigBodyProxy[] BodyProxies;

        [ProtoMember(9)][LabelText("击退抗性(可空)")][Tooltip("按 ConfigHitPattern.HitImpulseType 缩放外部击退冲量；未匹配到的类型按 1 处理。0=免疫击退")]
        public ConfigHitImpulseResist[] HitImpulseResists;
    }
}