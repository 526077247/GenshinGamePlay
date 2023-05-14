using Nino.Serialization;

namespace TaoTie
{
    [NinoSerialize]
    public partial class EnableObjView: ConfigAbilityAction
    {
        [NinoMember(10)]
        public bool SetEnable;
        protected override void Execute(Entity applier, ActorAbility ability, ActorModifier modifier, Entity target)
        {
            
            CombatComponent holderComponent = target.GetComponent<CombatComponent>();
            holderComponent?.EnableObjView(SetEnable).Coroutine();
            
        }
    }
}