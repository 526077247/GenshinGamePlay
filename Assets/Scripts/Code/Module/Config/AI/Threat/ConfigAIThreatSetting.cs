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
    public partial class ConfigAIThreatSetting
    {
        [LabelText("启用")]
        [ProtoMember(1, IsRequired = true)]
        public bool Enable = true;
        [ProtoMember(2, IsRequired = true)][MinValue(0.1f)][LabelText("超过目标距离范围清除威胁值")]
        public float ClearThreatTargetDistance = 10;
        [ProtoMember(3, IsRequired = true)][LabelText("超过出生地范围清除威胁值")]
        public float ClearThreatEdgeDistance = 100;
        [ProtoMember(4, IsRequired = true)][LabelText("超过范围清除威胁值倒计时（ms）")]
        public int ClearThreatTimerByDistance = 3000;
        
        [ProtoMember(5)][LabelText("无法寻路清除威胁值")]
        public bool ClearThreatByLostPath;
        [ProtoMember(6, IsRequired = true)][ShowIf(nameof(ClearThreatByLostPath))][LabelText("无法寻路清除威胁值倒计时（ms）")]
        public int ClearThreatTimerByLostPath = 3000;
        
        [ProtoMember(7)][LabelText("离开区域清除威胁值")]
        public bool ClearThreatByTargetOutOfZone;
        [ProtoMember(8, IsRequired = true)][ShowIf(nameof(ClearThreatByTargetOutOfZone))][LabelText("离开区域清除威胁值倒计时（ms）")]
        public int ClearThreatTimerByTargetOutOfZone = 3000;
        
        [ProtoMember(9, IsRequired = true)][LabelText("视觉感知附加威胁值")]
        public float ViewThreatGrow = 100f;
        [ProtoMember(10, IsRequired = true)][LabelText("听觉感知附加威胁值")]
        public float HearThreatGrow = 100f;
        [ProtoMember(11, IsRequired = true)][LabelText("感觉感知附加威胁值")]
        public float FeelThreatGrow = 500f;
        [ProtoMember(12, IsRequired = true)][LabelText("威胁值衰减速度（每秒）")]
        public float ThreatDecreaseSpeed = 30f;
        [ProtoMember(13)][LabelText("附加威胁值广播范围")]
        public float ThreatBroadcastRange;

        //[ProtoMember(14)]//todo:
        [LabelText("视觉感知距离衰减曲线")][HideReferenceObjectPicker]
        public AnimationCurve ViewAttenuationCurve;
        //[ProtoMember(15)]//todo:
        [LabelText("听觉感知距离衰减曲线")][HideReferenceObjectPicker]
        public AnimationCurve HearAttenuationCurve;
    }
}