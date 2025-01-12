using Nino.Core;

namespace TaoTie
{
    [NinoType(false)]
    public partial class ModifyAbility: ConfigAbilityAction
    {
        [NinoMember(10)]
        public string Key;
        [NinoMember(11)]
        public float Value;
        protected override void Execute(Entity applier, ActorAbility ability, ActorModifier modifier, Entity target)
        {
            ability.SetSpecials(Key, Value);
        }
    }
}