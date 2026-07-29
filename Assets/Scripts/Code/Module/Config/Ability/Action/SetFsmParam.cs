using ProtoBuf;

namespace TaoTie
{
    [ProtoContract]
    [ProtoInclude(100, typeof(SetFsmInt))]
    [ProtoInclude(101, typeof(SetFsmFloat))]
    [ProtoInclude(102, typeof(SetFsmBool))]
    [ProtoInclude(103, typeof(SetFsmTrigger))]
    public abstract class SetFsmParam<T> : ConfigAbilityAction where T: unmanaged
    {
        [ProtoMember(10)]
        public string Key;
        [ProtoMember(11)]
        public T Value;

        protected override void Execute(Entity actionExecuter, ActorAbility ability, ActorModifier modifier, Entity target)
        {
            FsmComponent fc = target.GetComponent<FsmComponent>();
            if (fc != null)
            {
                SetData(fc);
            }
        }

        protected abstract void SetData(FsmComponent fsmComponent);
    }
}