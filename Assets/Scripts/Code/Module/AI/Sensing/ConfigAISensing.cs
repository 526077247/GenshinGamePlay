using Nino.Serialization;
using Sirenix.OdinInspector;

namespace TaoTie
{
    [NinoSerialize]
    public partial class ConfigAISensing
    {
        [LabelText("启用")]
        public bool enable;
        [LabelText("敏感性")]
        public float sensitivity;
        [LabelText("启用视觉")]
        public bool enableVision;
        [LabelText("可视范围")]
        public float viewRange;
        [LabelText("视觉全角")]
        public bool viewPanoramic;
        [LabelText("水平FOV")]
        public float horizontalFov;
        [LabelText("垂直FOV")]
        public float verticalFov;
        [LabelText("感知范围")]
        public float feelRange;
        [LabelText("其他声音感知范围")]
        public float hearAttractionRange = 20f;
        [LabelText("脚步声音感知范围")]
        public float hearFootstepRange;
        [LabelText("无源受击感知范围")]
        public float sourcelessHitAttractionRange = 0f;
    }
}