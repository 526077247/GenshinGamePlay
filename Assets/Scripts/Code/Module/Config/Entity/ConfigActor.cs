using Nino.Core;
using Sirenix.OdinInspector;

namespace TaoTie
{
    [NinoType(false)]
    public partial class ConfigActor
    {
#if UNITY_EDITOR
        [PropertyOrder(int.MinValue + 1)] [LabelText("策划备注")]
        public string Remarks;
#endif
        [NinoMember(7)]
#if UNITY_EDITOR
        [ValueDropdown("@"+nameof(OdinDropdownHelper)+"."+nameof(OdinDropdownHelper.GetAbilities)+"()",AppendNextDrawer = true)]
#endif
        public string[] Abilities;
        [NinoMember(1)]
        public ConfigActorCommon Common = new ConfigActorCommon();
        [NinoMember(3)]
        public ConfigCombat Combat;
        [NinoMember(4)]
        public ConfigEquipController EquipController;
        [NinoMember(5)]
        public ConfigBillboard Billboard;
        [NinoMember(6)]
        public ConfigIntee Intee;
        [NinoMember(8)][NotNull]
        public ConfigModel Model = new ConfigSingletonModel();
    }
}