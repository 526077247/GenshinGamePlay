using Nino.Serialization;
using Sirenix.OdinInspector;

namespace TaoTie
{
    [NinoSerialize]
    public partial class ConfigActor
    {
        [NinoMember(7)]
#if UNITY_EDITOR
        [ValueDropdown("@"+nameof(OdinDropdownHelper)+"."+nameof(OdinDropdownHelper.GetAbilities)+"()",AppendNextDrawer = true)]
#endif
        public string[] Abilities;
        [NinoMember(1)]
        public ConfigActorCommon Common;
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