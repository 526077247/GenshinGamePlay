using Nino.Core;
using Sirenix.OdinInspector;

namespace TaoTie
{
    [NinoType(false)]
    public partial class ConfigHitImpulse
    {
        [NinoMember(1)][LabelText("击打力度等级")]
        public HitLevel HitLevel;
        [NinoMember(2)]
        public BaseValue HitImpulseX = new SingleValue();
        [NinoMember(3)]
        public BaseValue HitImpulseY = new SingleValue();
    }
}