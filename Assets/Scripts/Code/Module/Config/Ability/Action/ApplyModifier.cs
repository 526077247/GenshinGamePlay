using Nino.Core;

namespace TaoTie
{
    /// <summary>
    /// 应用Modifier，不会随别人移除
    /// </summary>
    [NinoType(false)]
    public partial class ApplyModifier: ConfigAbilityAction
    {
        [NinoMember(10)]
        public string ModifierName;

        protected override void Execute(Entity actionExecuter, ActorAbility ability, ActorModifier modifier, Entity target)
        {
            var ac = target.GetComponent<AbilityComponent>();
            if (ac != null)
            {
                ac.ApplyModifier(actionExecuter.Id, ability, ModifierName);
            }
        }
    }
}