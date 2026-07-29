using ProtoBuf;
using Sirenix.OdinInspector;

namespace TaoTie
{
    [ProtoContract]
    public partial class ConfigCombatProperty
    {
        [ProtoMember(1)]
#if UNITY_EDITOR
        [ValueDropdown("@"+nameof(OdinDropdownHelper)+"."+nameof(OdinDropdownHelper.GetNumericTypeId)+"()")]
#endif
        public int NumericType;
        [ProtoMember(2)]
        public float Value;
    }
}