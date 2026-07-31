using ProtoBuf;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using TaoTie.Inspector;
#endif
using UnityEngine;

namespace TaoTie
{
    [ProtoContract]
    public partial class ConfigActor
    {
#if UNITY_EDITOR
        [PropertyOrder(int.MinValue + 1)] [LabelText("策划备注")]
        public string Remarks;
#endif
        [PropertyOrder(int.MinValue + 2)][LabelText("Actor模板类型")][ProtoMember(10)]
        public ActorType Type;
        [ProtoMember(7)]
#if UNITY_EDITOR
        [ValueDropdown("@"+nameof(OdinDropdownHelper)+"."+nameof(OdinDropdownHelper.GetAbilities)+"()",AppendNextDrawer = true)]
#endif
        public string[] Abilities;
        [ProtoMember(1, IsRequired = true)]
        public ConfigActorCommon Common = new ConfigActorCommon();
        [ProtoMember(3)]
        public ConfigCombat Combat;
        [ProtoMember(12)][ShowIf("@"+nameof(Type)+"!=ActorType."+nameof(ActorType.Gadget))]
        public ConfigSkill Skill;
        [ProtoMember(4)][ShowIf("@"+nameof(Type)+"!=ActorType."+nameof(ActorType.Gadget))]
        public ConfigEquipController EquipController;
        [ProtoMember(5)]
        public ConfigBillboard Billboard;
        [ProtoMember(6)][ShowIf(nameof(Type),ActorType.Gadget)]
        public ConfigIntee Intee;
        [ProtoMember(8, IsRequired = true)][NotNull]
        public ConfigModel Model = new ConfigSingletonModel();
        [ProtoMember(9)][ShowIf("@"+nameof(Type)+"!=ActorType."+nameof(ActorType.Avatar))]
        public ConfigTrigger Trigger;
        [ProtoMember(11)]
        public ConfigMove Move;
    }
}