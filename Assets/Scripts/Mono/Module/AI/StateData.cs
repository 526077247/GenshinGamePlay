using ProtoBuf;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [ProtoContract]
    public class StateData
    {
        [TabGroup("Pose")][ProtoMember(1)]
        public bool IsDynamic;
        [ShowIf("@IsDynamic")][TabGroup("Pose")][ProtoMember(2)]
        public string Key;
        [ShowIf("@!IsDynamic")][TabGroup("Pose")][ProtoMember(3)]
        public int PoseID;
        [LabelText("是否能移动")][TabGroup("FSM")][ProtoMember(4)]
        public bool CanMove;
        [LabelText("是否能转向")][TabGroup("FSM")][ProtoMember(5)]
        public bool CanTurn;
        [LabelText("是否能跳跃")][TabGroup("FSM")][ProtoMember(6)]
        public bool CanJump;
        [LabelText("是否是跳跃")][TabGroup("FSM")][ProtoMember(7)]
        public bool IsJump;
        [LabelText("是否展示武器")][TabGroup("FSM")][ProtoMember(8)]
        public bool ShowWeapon;
        [LabelText("是否使用RagDoll")][TabGroup("FSM")][ProtoMember(9)]
        public bool UseRagDoll;
        [LabelText("*受速度影响")][TabGroup("FSM")][ProtoMember(10)][Tooltip("一般Run,Walk动作需要勾上")]
        public bool EffectBySpeed;
    }
}