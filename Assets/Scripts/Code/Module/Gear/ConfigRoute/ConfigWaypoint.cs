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
        public int index;
        [LabelText("坐标")]
        [NinoMember(1)] public Vector3 pos;
        [LabelText("到达该点后等待时间")]
        [NinoMember(2)] public float waitTime;

        [DisableInEditorMode]
        [LabelText("使用动画移动")]
        [NinoMember(3)]public bool useAnimMove;//todo:
        [ShowIf("@useAnimMove")]
        [NinoMember(4)] public int speedLevel;
        [LabelText("移动速度（m/s）")]
        [ShowIf("@!useAnimMove")]
        [NinoMember(5)] public float targetVelocity;

        [LabelText("有最终抵达事件")]
        [NinoMember(6)] public bool hasReachEvent;
        [LabelText("有角色靠近事件")][DisableIf("reachStop")]
        [NinoMember(7)] public bool hasHeroNearEvent;
        [LabelText("到达停止(默认有角色靠近事件)")][OnValueChanged(nameof(Reset))]
        [NinoMember(8)] public bool reachStop;
        
        [LabelText("行进到等待开始时的方向")][BoxGroup("旋转指定圈数参数")]
        [NinoMember(9)] public Vector3 rotRoundReachDir;
        [LabelText("行进中旋转的圈数")][BoxGroup("旋转指定圈数参数")]
        [NinoMember(10)] public int rotRoundReachRounds;
        
        [LabelText("等待时旋转结束的方向")][BoxGroup("旋转指定圈数参数")]
        [NinoMember(11)] public Vector3 rotRoundLeaveDir;
        [LabelText("等待时旋转的圈数")][BoxGroup("旋转指定圈数参数")]
        [NinoMember(12)] public int rotRoundWaitRounds;
        
        [LabelText("移动中时，转圈角速度（度/s）")][BoxGroup("按角速度旋转参数")]
        [NinoMember(13)] public float rotAngleMoveSpeed;
        [LabelText("等待中时，转圈角速度（度/s）")][BoxGroup("按角速度旋转参数")]
        [NinoMember(14)] public float rotAngleWaitSpeed;
        [LabelText("是否转一圈就停止转圈？")][BoxGroup("按角速度旋转参数")]
        [NinoMember(15)] public bool rotAngleSameStop;
        private void Reset()
        {
            if (reachStop)
            {
                hasHeroNearEvent = true;
            }
        }
    }
}