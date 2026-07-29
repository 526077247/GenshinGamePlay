using ProtoBuf;

namespace TaoTie
{
    [ProtoContract]
    public partial class AddSkillInfo: ConfigAbilityAction
    {
        [ProtoMember(10)]
        public ConfigSkillInfo Skill;

        
        protected override void Execute(Entity actionExecuter, ActorAbility ability, ActorModifier modifier, Entity target)
        {
            var cc = target.GetComponent<SkillComponent>();
            if (cc != null)
            {
                cc.AddSkillInfo(Skill);
            }
        }
    }
}