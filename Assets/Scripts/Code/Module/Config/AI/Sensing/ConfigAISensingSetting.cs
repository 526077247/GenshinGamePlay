using Nino.Serialization;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [NinoSerialize]
    public class ConfigAISensingSetting
    {
        [LabelText("敏感性")][Min(0)]
        public float Sensitivity = 1;
        [LabelText("启用视觉")]
        public bool EnableVision;
        [LabelText("可视范围(m)")][ShowIf(nameof(EnableVision))]
        public float ViewRange = 30;
        [LabelText("视觉全角")][ShowIf(nameof(EnableVision))]
        public bool ViewPanoramic;
        [LabelText("水平FOV(1-360°)")][ShowIf("@"+nameof(EnableVision)+"&&!"+nameof(ViewPanoramic))]
        public float HorizontalFov;
        [LabelText("垂直FOV(1-360°)")][ShowIf("@"+nameof(EnableVision)+"&&!"+nameof(ViewPanoramic))]
        public float VerticalFov;
        [LabelText("感知范围(m)")]
        public float FeelRange = 10;
        [LabelText("其他声音感知范围(m)")]
        public float HearAttractionRange = 20f;
        [LabelText("脚步声音感知范围(m)")]
        public float HearFootstepRange;
        [LabelText("无源受击感知范围(m)")]
        public float SourcelessHitAttractionRange = 0f;

        public ConfigAISensingSetting DeepCopy()
        {
            ConfigAISensingSetting res = new ConfigAISensingSetting();
            res.Sensitivity = Sensitivity;
            res.EnableVision = EnableVision;
            res.ViewRange = Mathf.Clamp(ViewRange,0,200);
            res.ViewPanoramic = ViewPanoramic;
            res.HorizontalFov = Mathf.Clamp(HorizontalFov,1,360);
            res.VerticalFov = Mathf.Clamp(VerticalFov,1,360);
            res.FeelRange = FeelRange;
            res.HearAttractionRange = HearAttractionRange;
            res.HearFootstepRange = HearFootstepRange;
            res.SourcelessHitAttractionRange = SourcelessHitAttractionRange;
            return res;
        }
    }
}