using System;
using DaGenGraph;
using ProtoBuf;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [ProtoContract]
    public partial class ConfigWaypoint
    {
        [ProtoMember(17)][DisableInEditorMode]
        public int Index;
        
        [ProtoMember(16, IsRequired = true)][LabelText("是否是相对坐标、方向")]
        public bool IsLocal = true;
        [LabelText("坐标")]
        [ProtoMember(1)] public Vector3 Pos;
        [LabelText("到达该点后等待时间")]
        [ProtoMember(2)] public float WaitTime;

        [DisableInEditorMode]
        [LabelText("使用动画移动")]
        [ProtoMember(3)]public bool UseAnimMove;//todo:
        [ShowIf("@"+nameof(UseAnimMove))]
        [ProtoMember(4)] public int SpeedLevel;
        [LabelText("移动速度（m/s）")]
        [ShowIf("@!"+nameof(UseAnimMove))][MinValue(0.1f)]
        [ProtoMember(5, IsRequired = true)] public float TargetVelocity = 2;

        [LabelText("有最终抵达事件")]
        [ProtoMember(6)] public bool HasReachEvent;
        [LabelText("有角色靠近事件")][DisableIf(nameof(ReachStop))]
        [ProtoMember(7)] public bool HasAvatarNearEvent;
        [LabelText("到达停止(默认有角色靠近事件)")][OnValueChanged(nameof(Reset))]
        [ProtoMember(8)] public bool ReachStop;
        
        [LabelText("行进到等待开始时的方向")][BoxGroup("旋转指定圈数参数")]
        [ProtoMember(9)] public Vector3 RotRoundReachDir;
        [LabelText("行进中旋转的圈数")][BoxGroup("旋转指定圈数参数")]
        [ProtoMember(10)] public int RotRoundReachRounds;
        
        [LabelText("等待时旋转结束的方向")][BoxGroup("旋转指定圈数参数")]
        [ProtoMember(11)] public Vector3 RotRoundLeaveDir;
        [LabelText("等待时旋转的圈数")][BoxGroup("旋转指定圈数参数")]
        [ProtoMember(12)] public int RotRoundWaitRounds;
        
        [LabelText("移动中时，转圈角速度（度/s）")][BoxGroup("按角速度旋转参数")]
        [ProtoMember(13)] public float RotAngleMoveSpeed;
        [LabelText("等待中时，转圈角速度（度/s）")][BoxGroup("按角速度旋转参数")]
        [ProtoMember(14)] public float RotAngleWaitSpeed;
        [LabelText("是否转一圈就停止转圈？")][BoxGroup("按角速度旋转参数")]
        [ProtoMember(15)] public bool RotAngleSameStop;
        private void Reset()
        {
            if (ReachStop)
            {
                HasAvatarNearEvent = true;
            }
        }

        public Vector3 GetPosition(SceneGroup sceneGroup)
        {
            if (IsLocal && sceneGroup != null)
            {
                return Quaternion.Euler(sceneGroup.Rotation) * Pos + sceneGroup.Position;
            }
            return Pos;
        }
        
        public Vector3 GetRotRoundReachDir(SceneGroup sceneGroup)
        {
            if (IsLocal && sceneGroup != null)
            {
                return Quaternion.Euler(sceneGroup.Rotation) * RotRoundReachDir + sceneGroup.Position;
            }
            return RotRoundReachDir;
        }
        
        public Vector3 GetRotRoundLeaveDir(SceneGroup sceneGroup)
        {
            if (IsLocal && sceneGroup != null)
            {
                return Quaternion.Euler(sceneGroup.Rotation) * RotRoundLeaveDir + sceneGroup.Position;
            }
            return RotRoundLeaveDir;
        }
    }
}