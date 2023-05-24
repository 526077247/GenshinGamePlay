using Nino.Serialization;
using Sirenix.OdinInspector;

namespace TaoTie
{
    [NinoSerialize]
    public partial class ConfigEntity
    {
        [NinoMember(7)][ValueDropdown("@OdinDropdownHelper.GetAbilities()")]
        public string[] Abilities;
        [NinoMember(1)]
        public ConfigEntityCommon Common;
        [NinoMember(3)]
        public ConfigCombat Combat;
        [NinoMember(4)]
        public ConfigEquipController EquipController;
        [NinoMember(5)]
        public ConfigBillboard Billboard;
        [NinoMember(6)]
        public ConfigIntee Intee;
    }
}