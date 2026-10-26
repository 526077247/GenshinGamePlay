using ProtoBuf;
using UnityEngine;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using TaoTie.Inspector;
#endif

namespace TaoTie
{
    [ProtoContract][LabelText("动画驱动移动")]
    public partial class ConfigAnimatorMove: ConfigMoveAgent
    {
        [ProtoMember(10, IsRequired = true)]
        public FacingMoveType FacingMove = FacingMoveType.FourDirection;

        [ProtoMember(11)][LabelText("跳跃初速度")][Tooltip("起跳期间锁定的上升速度，决定跳跃高度")]
        public float JumpSpeed = 5f;
        [ProtoMember(12)][LabelText("起跳锁定时间(s)")][Tooltip("起跳后保持跳跃初速度的时长，模拟长按跳跃")]
        public float JumpDuration = 0.2f;
        [ProtoMember(13)][LabelText("重力加速度")]
        public float Gravity = 30f;
        [ProtoMember(14)][LabelText("空气摩擦(u/s)")][Tooltip("离地后水平速度的线性衰减速率")]
        public float AirFriction = 0.5f;
        [ProtoMember(15)][LabelText("击退速度倍率")][Tooltip("击退冲量转换为速度时的倍率")]
        public float HitImpulseScale = 5f;
        [ProtoMember(16)][LabelText("击退衰减(u/s)")][Tooltip("击退速度的线性衰减速率，竖直分量同样生效")]
        public float HitImpulseDecay = 50f;
        [ProtoMember(17)][LabelText("台阶高度")][Tooltip("着地检测向脚下延伸的高度，用于台阶/斜坡")]
        public float StepHeight = 0.15f;
        [ProtoMember(18)][LabelText("可站立最大坡度(°)")]
        public float SlopeLimit = 80f;
        [ProtoMember(19)][LabelText("地面检测余量")]
        public float GroundSkin = 0.05f;
        [ProtoMember(20)][LabelText("是否可以穿墙")][Tooltip("false 时移动会被墙体扫掠阻挡")]
        public bool CanPassWall;
        [ProtoMember(21)][LabelText("墙体碰撞层")][Tooltip("CanPassWall 为 false 时，墙体扫掠使用的碰撞层")]
        public LayerMask WallLayer = LayerMask.GetMask("Default", "HitScene");
        [ProtoMember(22)][LabelText("地面检测碰撞层")][Tooltip("着地检测使用的碰撞层，自身碰撞体自动忽略")]
        public LayerMask GroundMask = LayerMask.GetMask("Default", "HitScene", "Water", "Entity");
    }
}
