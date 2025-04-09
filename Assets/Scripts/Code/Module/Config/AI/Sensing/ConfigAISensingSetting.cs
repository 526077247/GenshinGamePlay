using Nino.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [NinoType(false)]
    public class ConfigAISensingSetting
    {
        [LabelText("*敏感性")][MinValue(0.01f)][Tooltip("作为系数乘以可视范围等各种感知范围")]
        public float Sensitivity = 1;
        [LabelText("启用视觉")]
        public bool EnableVision;
        [LabelText("可视范围(m)")][ShowIf(nameof(EnableVision))][MinValue(0)][Range(0,200)]
        public float ViewRange = 30;
        [LabelText("视觉全角")][ShowIf(nameof(EnableVision))]
        public bool ViewPanoramic;
        [LabelText("水平FOV(1-360°)")][ShowIf("@"+nameof(EnableVision)+"&&!"+nameof(ViewPanoramic))][Range(1,360)]
        public float HorizontalFov = 90;
        [LabelText("垂直FOV(1-360°)")][ShowIf("@"+nameof(EnableVision)+"&&!"+nameof(ViewPanoramic))][Range(1,360)]
        public float VerticalFov = 90;
        [LabelText("感知范围(m)")][MinValue(0)]
        public float FeelRange = 10;
        [LabelText("其他声音感知范围(m)")][MinValue(0)]
        public float HearAttractionRange = 20f;
        [LabelText("脚步声音感知范围(m)")][MinValue(0)]
        public float HearFootstepRange;
        [LabelText("无源受击感知范围(m)")][MinValue(0)]
        public float SourcelessHitAttractionRange;

        public ConfigAISensingSetting DeepCopy()
        {
            ConfigAISensingSetting res = new ConfigAISensingSetting();
            res.Sensitivity = Sensitivity;
            res.EnableVision = EnableVision;
            res.ViewRange = ViewRange;
            res.ViewPanoramic = ViewPanoramic;
            res.HorizontalFov = HorizontalFov;
            res.VerticalFov = VerticalFov;
            res.FeelRange = FeelRange;
            res.HearAttractionRange = HearAttractionRange;
            res.HearFootstepRange = HearFootstepRange;
            res.SourcelessHitAttractionRange = SourcelessHitAttractionRange;
            return res;
        }
    }
}