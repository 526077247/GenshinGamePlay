using Nino.Serialization;

namespace TaoTie
{
    [NinoSerialize]
    public partial class EnableRenderer: ConfigAbilityAction
    {
        [NinoMember(10)]
        public bool SetEnable;
        protected override void Execute(Entity applier, ActorAbility ability, ActorModifier modifier, Entity target)
        {
            GameObjectHolderComponent holderComponent = target.GetComponent<GameObjectHolderComponent>();
            holderComponent?.EnableRenderer(SetEnable).Coroutine();
        }
    }
}