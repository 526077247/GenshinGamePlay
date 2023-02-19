using Nino.Serialization;
using Sirenix.OdinInspector;

namespace TaoTie
{
    [NinoSerialize]
    public partial class ConfigCombatProperty
    {
        [NinoMember(1)][ValueDropdown("@OdinDropdownHelper.GetNumericTypeId()")]
        public int NumericType;
        [NinoMember(2)]
        public float Value;
    }
}