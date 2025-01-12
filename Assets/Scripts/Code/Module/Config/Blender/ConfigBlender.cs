using Nino.Core;
using Sirenix.OdinInspector;

namespace TaoTie
{
    [NinoType(false)]
    public partial class ConfigBlender
    {
        [NinoMember(2)]
        public EasingFunction.Ease Ease = EasingFunction.Ease.Linear;

        [NinoMember(1)][LabelText("过渡时间(ms)")]
        public int DeltaTime = 1000;
    }
}