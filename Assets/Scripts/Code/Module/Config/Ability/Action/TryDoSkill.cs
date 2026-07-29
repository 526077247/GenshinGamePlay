using ProtoBuf;
using Sirenix.OdinInspector;

namespace TaoTie
{
    [ProtoContract]
    public partial class TryDoSkill: ConfigAbilityAction
    {
        [ProtoMember(10, IsRequired = true)][LabelText("是否角色唯一技能Id")]
        public bool IsLocalId = false;
        [ProtoMember(11)][LabelText("技能配置表Id")][ShowIf("@!"+nameof(IsLocalId))]
        public int ConfigId;
        [ProtoMember(12)][LabelText("当前角色唯一技能Id")][ShowIf(nameof(IsLocalId))]
        public int LocalId;
        protected override void Execute(Entity actionExecuter, ActorAbility ability, ActorModifier modifier, Entity target)
        {
            var cc = target.GetComponent<SkillComponent>();
            if (cc != null)
            {
                if (IsLocalId)
                {
                    cc.TryDoSkillById(LocalId);
                }
                else
                {
                    cc.TryDoSkill(ConfigId);
                }
               
            }
        }
    }
}