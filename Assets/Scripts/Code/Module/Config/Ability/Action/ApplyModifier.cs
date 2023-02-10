using Nino.Serialization;

namespace TaoTie
{
    /// <summary>
    /// 应用Modifier，不会随别人移除
    /// </summary>
    [NinoSerialize]
    public class ApplyModifier: ConfigAbilityAction
    {
        [NinoMember(10)]
        public string ModifierName;

        protected override void Execute(Entity applier, ActorAbility ability, ActorModifier modifier, Entity target)
        {
            var ac = target.GetComponent<AbilityComponent>();
            if (ac != null)
            {
                ac.ApplyModifier(applier.Id, ability, ModifierName);
            }
        }
    }
}