using Nino.Core;
using Sirenix.OdinInspector;

namespace TaoTie
{
    [NinoType(false)]
    public partial class TryDoSkill: ConfigAbilityAction
    {
        [NinoMember(10)][LabelText("是否角色唯一技能Id")]
        public bool IsLocalId = false;
        [NinoMember(11)][LabelText("技能配置表Id")][ShowIf("@!"+nameof(IsLocalId))]
        public int ConfigId;
        [NinoMember(12)][LabelText("当前角色唯一技能Id")][ShowIf(nameof(IsLocalId))]
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