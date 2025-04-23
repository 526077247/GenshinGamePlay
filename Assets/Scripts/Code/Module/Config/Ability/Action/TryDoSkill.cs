using Nino.Core;

namespace TaoTie
{
    [NinoType(false)]
    public partial class TryDoSkill: ConfigAbilityAction
    {
        [NinoMember(10)]
        public int SkillId;
        
        protected override void Execute(Entity actionExecuter, ActorAbility ability, ActorModifier modifier, Entity target)
        {
            var cc = target.GetComponent<SkillComponent>();
            if (cc != null)
            {
                cc.TryDoSkill(SkillId);
            }
        }
    }
}