using ProtoBuf;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using TaoTie.Inspector;
#endif
using UnityEngine;

namespace TaoTie
{
    /// <summary>
    /// 角色体积代理：模型空间下的一个水平圆 + 竖直有效区间，用于角色间互斥（穿模阻挡 / 动量推挤）。
    ///
    /// 为什么用「圆 + 竖直区间」而不是物理碰撞体：
    /// 1. 单位位置是逻辑直写（SceneEntity.Position），物理世界的 static collider 与它不同步
    ///    （工程 m_AutoSyncTransforms = 0，查询会有 1 物理步滞后）；
    /// 2. 部分资产没有碰撞体（如 MuJian），基于物理层的方案会漏掉它；
    /// 3. 半径需要按体型逐单位配置，而物理胶囊半径与 ModelRadius(墙扫掠膨胀半径) 口径不同。
    ///
    /// 为什么偏移写在模型空间、随 yaw 旋转、与动画无关：
    /// 读骨骼会被动画带着漂移（抬脚/跨步时 proxy 乱跑），静止配置才可控。
    ///
    /// 「钻裆」靠竖直区间表达，不是靠少放代理：
    /// 例：恐龙 4 条腿 yBottom=0 / YTop=1.6 与玩家(0~1.5) 交叠 → 挡；
    ///     腹部代理 YBottom=2.2 高于玩家头顶 → 不挡，玩家可从肚子下穿过。
    /// </summary>
    [ProtoContract]
    public partial class ConfigBodyProxy
    {
        [ProtoMember(1)][LabelText("局部偏移X")][Tooltip("模型空间，随实体 yaw 旋转；Y 由竖直区间决定")]
        public float OffsetX;
        [ProtoMember(2)][LabelText("局部偏移Z")]
        public float OffsetZ;
        [ProtoMember(3)][LabelText("半径")][Tooltip("水平截面半径；多足模型的脚建议只取脚掌半径，不要为动画步幅额外膨胀（膨胀会吃掉钻裆缝隙）")][MinValue(0.01f)]
        public float Radius = 0.3f;
        [ProtoMember(4)][LabelText("底部高度")][Tooltip("相对实体脚下的世界 y 偏移")]
        public float YBottom;
        [ProtoMember(5)][LabelText("顶部高度")][Tooltip("相对实体脚下的世界 y 偏移；与对方区间不交叠时完全不阻挡（用于钻裆）")]
        public float YTop = 1.5f;
    }
}
