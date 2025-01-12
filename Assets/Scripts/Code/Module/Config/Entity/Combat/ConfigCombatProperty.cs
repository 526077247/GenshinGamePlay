using Nino.Core;
using Sirenix.OdinInspector;

namespace TaoTie
{
    [NinoType(false)]
    public partial class ConfigCombatProperty
    {
        [NinoMember(1)]
#if UNITY_EDITOR
        [ValueDropdown("@"+nameof(OdinDropdownHelper)+"."+nameof(OdinDropdownHelper.GetNumericTypeId)+"()")]
#endif
        public int NumericType;
        [NinoMember(2)]
        public float Value;
    }
}