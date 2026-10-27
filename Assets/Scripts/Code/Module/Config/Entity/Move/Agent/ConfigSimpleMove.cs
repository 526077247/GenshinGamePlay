using ProtoBuf;
using UnityEngine;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using TaoTie.Inspector;
#endif

namespace TaoTie
{
    [ProtoContract][LabelText("速度驱动移动")]
    public partial class ConfigSimpleMove: ConfigMoveAgent
    {
        [ProtoMember(1)][LabelText("是否可以穿墙")][Tooltip("false 时移动会被墙体扫掠阻挡")]
        public bool CanPassWall;
        [ProtoMember(2, IsRequired = true)][LabelText("墙体碰撞层")][Tooltip("CanPassWall 为 false 时，墙体扫掠使用的碰撞层")]
        public LayerMask WallLayer = LayerMask.GetMask("Default");
        // 3/4 原为 HitImpulseScale / HitImpulseDecay，已上移到 ConfigMoveAgent（30/31）统一管理。
        // 旧 bytes 里残留的 3/4 会被当作未知字段跳过，取值回落到基类默认 5/50，与原值一致。
    }
}