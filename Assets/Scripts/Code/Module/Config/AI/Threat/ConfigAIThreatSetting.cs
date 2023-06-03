using Nino.Serialization;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [NinoSerialize]
    public partial class ConfigAIThreatSetting
    {
        [LabelText("启用")]
        [NinoMember(1)]
        public bool Enable = true;
        [NinoMember(2)][Min(0.1f)][LabelText("超过范围清除威胁值")]
        public float ClearThreatTargetDistance = 10;
        [NinoMember(3)][LabelText("超过防守区域边缘距离清除威胁值")]
        public float ClearThreatEdgeDistance = 1;
        [NinoMember(4)][LabelText("超过范围清除威胁值倒计时（ms）")]
        public int ClearThreatTimerByDistance = 3000;
        
        [NinoMember(5)][LabelText("无法寻路清除威胁值")]
        public bool ClearThreatByLostPath;
        [NinoMember(6)][ShowIf(nameof(ClearThreatByLostPath))][LabelText("无法寻路清除威胁值倒计时（ms）")]
        public int ClearThreatTimerByLostPath = 3000;
        
        [NinoMember(6)][LabelText("离开区域清除威胁值")]
        public bool ClearThreatByTargetOutOfZone;
        [NinoMember(8)][ShowIf(nameof(ClearThreatByTargetOutOfZone))][LabelText("离开区域清除威胁值倒计时（ms）")]
        public int ClearThreatTimerByTargetOutOfZone = 3000;
        
        [NinoMember(9)][LabelText("视觉感知附加威胁值")]
        public float ViewThreatGrow = 100f;
        [NinoMember(10)][LabelText("听觉感知附加威胁值")]
        public float HearThreatGrow = 100f;
        [NinoMember(11)][LabelText("感觉感知附加威胁值")]
        public float FeelThreatGrow = 500f;
        [NinoMember(12)][LabelText("威胁值衰减速度（每秒）")]
        public float ThreatDecreaseSpeed = 30f;
        [NinoMember(13)][LabelText("附加威胁值广播范围")]
        public float ThreatBroadcastRange;

        [NinoMember(14)][LabelText("视觉感知距离衰减曲线")][HideReferenceObjectPicker]
        public AnimationCurve ViewAttenuationCurve;
        [NinoMember(15)][LabelText("听觉感知距离衰减曲线")][HideReferenceObjectPicker]
        public AnimationCurve HearAttenuationCurve;
    }
}