using ProtoBuf;

namespace TaoTie
{
    [ProtoContract]
    public partial class ModifyAbility: ConfigAbilityAction
    {
        [ProtoMember(10)]
        public string Key;
        [ProtoMember(11)]
        public float Value;
        protected override void Execute(Entity actionExecuter, ActorAbility ability, ActorModifier modifier, Entity target)
        {
            ability.SetSpecials(Key, Value);
        }
    }
}