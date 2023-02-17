using Nino.Serialization;
using Sirenix.OdinInspector;

namespace TaoTie
{
    [NinoSerialize]
    public class ModifierProperty
    {
        [NinoMember(1)][ValueDropdown("@OdinDropdownHelper.GetNumericTypeId()")]
        public int NumericType;
        [NinoMember(2)]
        public float Value;
    }
}