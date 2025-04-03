using Nino.Core;

namespace TaoTie
{
    [NinoType(false)]
    public partial class AddNumericValue: ConfigAbilityAction
    {
        [NinoMember(10)]
#if UNITY_EDITOR
        [Sirenix.OdinInspector.ValueDropdown("@"+nameof(OdinDropdownHelper)+"."+nameof(OdinDropdownHelper.GetNumericTypeId)+"()")]
#endif
        public int Key;
        [NinoMember(11)]
        public BaseValue Value = new ZeroValue();
        
        protected override void Execute(Entity applier, ActorAbility ability, ActorModifier modifier, Entity target)
        {
            var numC = target.GetComponent<NumericComponent>();
            if (numC != null)
            {
                //todo: 上下限判断
                var now = numC.GetAsFloat(Key);
                var next = now + Value.Resolve(target, ability);
                numC.Set(Key, next);
            }
        }
    }
}