using Nino.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [NinoType(false)]
    public partial class ConfigActor
    {
#if UNITY_EDITOR
        [PropertyOrder(int.MinValue + 1)] [LabelText("策划备注")]
        public string Remarks;
#endif
        [PropertyOrder(int.MinValue + 2)][LabelText("Actor模板类型")][NinoMember(10)]
        public ActorType Type;
        [NinoMember(7)]
#if UNITY_EDITOR
        [ValueDropdown("@"+nameof(OdinDropdownHelper)+"."+nameof(OdinDropdownHelper.GetAbilities)+"()",AppendNextDrawer = true)]
#endif
        public string[] Abilities;
        [NinoMember(1)]
        public ConfigActorCommon Common = new ConfigActorCommon();
        [NinoMember(3)]
        public ConfigCombat Combat;
        [NinoMember(4)][ShowIf("@"+nameof(Type)+"!=ActorType."+nameof(ActorType.Gadget))]
        public ConfigEquipController EquipController;
        [NinoMember(5)]
        public ConfigBillboard Billboard;
        [NinoMember(6)][ShowIf(nameof(Type),ActorType.Gadget)]
        public ConfigIntee Intee;
        [NinoMember(8)][NotNull]
        public ConfigModel Model = new ConfigSingletonModel();
        [NinoMember(9)][ShowIf("@"+nameof(Type)+"!=ActorType."+nameof(ActorType.Avatar))]
        public ConfigTrigger Trigger;
        [NinoMember(11)]
        public ConfigMove Move = new ConfigAnimatorMove();
    }
}