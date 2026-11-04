using ProtoBuf;
using UnityEngine;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using TaoTie.Inspector;
#endif

namespace TaoTie
{
    [ProtoContract]
    public partial class TryDoSkill: ConfigAbilityAction
    {
        [ProtoMember(10, IsRequired = true)][LabelText("是否按技能槽位释放")]
        public bool IsSlot = false;
        [ProtoMember(11)][LabelText("技能配置表Id")][ShowIf("@!"+nameof(IsSlot))]
        public int ConfigId;
        [ProtoMember(12)][LabelText("槽位号")][ShowIf(nameof(IsSlot))][Range(1, GameConst.SkillSlotConst)]
        public int Slot;
        protected override void Execute(Entity actionExecuter, ActorAbility ability, ActorModifier modifier, Entity target)
        {
            var cc = target.GetComponent<SkillComponent>();
            if (cc == null) return;
            if (IsSlot)
            {
                cc.TryDoSkillBySlot(Slot);
            }
            else
            {
                cc.TryDoSkill(ConfigId);
            }
        }
    }
}