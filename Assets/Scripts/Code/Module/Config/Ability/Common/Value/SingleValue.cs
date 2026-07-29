using ProtoBuf;

namespace TaoTie
{
    /// <summary>
    /// 固定值
    /// </summary>
    [ProtoContract]
    public partial class SingleValue: BaseValue
    {
        public SingleValue()
        {
            Value = 0;
        }
        public SingleValue(float val)
        {
            Value = val;
        }
        [ProtoMember(1)]
        public float Value;
        public override float Resolve(Entity entity,ActorAbility ability)
        {
            return Value;
        }
    }
}