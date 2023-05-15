using Nino.Serialization;
using Sirenix.OdinInspector;

namespace TaoTie
{
    [NinoSerialize]
    public partial class ConfigCameraBleed
    {
        [NinoMember(0)]
        public EasingFunction.Ease Ease = EasingFunction.Ease.Linear;

        [NinoMember(1)][LabelText("过渡时间(ms)")]
        public int DeltaTime = 1000;
    }
}