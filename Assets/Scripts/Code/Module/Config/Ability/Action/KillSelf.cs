using Nino.Serialization;

namespace TaoTie
{
    [NinoSerialize]
    public partial class KillSelf: ConfigAbilityAction
    {
        [NinoMember(10)]
        public DieStateFlag DieFlag;
        
        protected override void Execute(Entity applier, ActorAbility ability, ActorModifier modifier, Entity target)
        {
            target.GetComponent<CombatComponent>().DoKill(applier.Id, DieFlag);
        }
    }
}