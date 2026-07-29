using ProtoBuf;

namespace TaoTie
{
    [ProtoContract]
    public partial class AddNumericValue: ConfigAbilityAction
    {
        [ProtoMember(10)]
#if UNITY_EDITOR
        [Sirenix.OdinInspector.ValueDropdown("@"+nameof(OdinDropdownHelper)+"."+nameof(OdinDropdownHelper.GetNumericTypeId)+"()")]
#endif
        public int Key;
        [ProtoMember(11, IsRequired = true)]
        public BaseValue Value = new ZeroValue();
        
        protected override void Execute(Entity actionExecuter, ActorAbility ability, ActorModifier modifier, Entity target)
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