using System;
using Nino.Serialization;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [NinoSerialize]
    public partial class ConfigWaypoint
    {
        [DisableInEditorMode]
        public int Index;
        [LabelText("坐标")]
        [NinoMember(1)] public Vector3 Pos;
        [LabelText("到达该点后等待时间")]
        [NinoMember(2)] public float WaitTime;

        [DisableInEditorMode]
        [LabelText("使用动画移动")]
        [NinoMember(3)]public bool UseAnimMove;//todo:
        [ShowIf("@"+nameof(UseAnimMove))]
        [NinoMember(4)] public int SpeedLevel;
        [LabelText("移动速度（m/s）")]
        [ShowIf("@!"+nameof(UseAnimMove))][MinValue(0.1f)]
        [NinoMember(5)] public float TargetVelocity = 2;

        [LabelText("有最终抵达事件")]
        [NinoMember(6)] public bool HasReachEvent;
        [LabelText("有角色靠近事件")][DisableIf(nameof(ReachStop))]
        [NinoMember(7)] public bool HasHeroNearEvent;
        [LabelText("到达停止(默认有角色靠近事件)")][OnValueChanged(nameof(Reset))]
        [NinoMember(8)] public bool ReachStop;
        
        [LabelText("行进到等待开始时的方向")][BoxGroup("旋转指定圈数参数")]
        [NinoMember(9)] public Vector3 RotRoundReachDir;
        [LabelText("行进中旋转的圈数")][BoxGroup("旋转指定圈数参数")]
        [NinoMember(10)] public int RotRoundReachRounds;
        
        [LabelText("等待时旋转结束的方向")][BoxGroup("旋转指定圈数参数")]
        [NinoMember(11)] public Vector3 RotRoundLeaveDir;
        [LabelText("等待时旋转的圈数")][BoxGroup("旋转指定圈数参数")]
        [NinoMember(12)] public int RotRoundWaitRounds;
        
        [LabelText("移动中时，转圈角速度（度/s）")][BoxGroup("按角速度旋转参数")]
        [NinoMember(13)] public float RotAngleMoveSpeed;
        [LabelText("等待中时，转圈角速度（度/s）")][BoxGroup("按角速度旋转参数")]
        [NinoMember(14)] public float RotAngleWaitSpeed;
        [LabelText("是否转一圈就停止转圈？")][BoxGroup("按角速度旋转参数")]
        [NinoMember(15)] public bool RotAngleSameStop;
        private void Reset()
        {
            if (ReachStop)
            {
                HasHeroNearEvent = true;
            }
        }
    }
}