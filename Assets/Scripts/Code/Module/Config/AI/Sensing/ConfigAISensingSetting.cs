using Nino.Serialization;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [NinoSerialize]
    public class ConfigAISensingSetting
    {
        [LabelText("敏感性")]
        public float sensitivity;
        [LabelText("启用视觉")]
        public bool enableVision;
        [LabelText("可视范围(m)")][ShowIf(nameof(enableVision))]
        public float viewRange = 30;
        [LabelText("视觉全角")][ShowIf(nameof(enableVision))]
        public bool viewPanoramic;
        [LabelText("水平FOV(1-360°)")][ShowIf("@enableVision&&!viewPanoramic")]
        public float horizontalFov;
        [LabelText("垂直FOV(1-360°)")][ShowIf("@enableVision&&!viewPanoramic")]
        public float verticalFov;
        [LabelText("感知范围(m)")]
        public float feelRange = 10;
        [LabelText("其他声音感知范围(m)")]
        public float hearAttractionRange = 20f;
        [LabelText("脚步声音感知范围(m)")]
        public float hearFootstepRange;
        [LabelText("无源受击感知范围(m)")]
        public float sourcelessHitAttractionRange = 0f;

        public ConfigAISensingSetting DeepCopy()
        {
            ConfigAISensingSetting res = new ConfigAISensingSetting();
            res.sensitivity = sensitivity;
            res.enableVision = enableVision;
            res.viewRange = Mathf.Clamp(viewRange,0,200);
            res.viewPanoramic = viewPanoramic;
            res.horizontalFov = Mathf.Clamp(horizontalFov,1,360);
            res.verticalFov = Mathf.Clamp(verticalFov,1,360);
            res.feelRange = feelRange;
            res.hearAttractionRange = hearAttractionRange;
            res.hearFootstepRange = hearFootstepRange;
            res.sourcelessHitAttractionRange = sourcelessHitAttractionRange;
            return res;
        }
    }
}