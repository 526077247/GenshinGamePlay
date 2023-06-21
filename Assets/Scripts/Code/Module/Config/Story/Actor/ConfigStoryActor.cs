using Nino.Serialization;
using Sirenix.OdinInspector;

namespace TaoTie
{
    [NinoSerialize]
    public class ConfigStoryActor
    {
        [NinoMember(1)][LabelText("策划备注")]
        public string Remarks;
        [NinoMember(2)]
        public int Id;
        [NinoMember(3)]
#if UNITY_EDITOR
        [ValueDropdown("@"+nameof(OdinDropdownHelper)+"."+nameof(OdinDropdownHelper.GetCharacterConfigIds)+"()")]
#endif
        public int ConfigId;
    }
}