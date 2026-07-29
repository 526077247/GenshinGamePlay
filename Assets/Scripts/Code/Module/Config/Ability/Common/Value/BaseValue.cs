using ProtoBuf;

namespace TaoTie
{
    /// <summary>
    /// 值
    /// </summary>
    [ProtoContract]
    [ProtoInclude(100, typeof(AbilityValue))]
    [ProtoInclude(101, typeof(FormulaValue))]
    [ProtoInclude(102, typeof(NumericValue))]
    [ProtoInclude(103, typeof(OperatorValue))]
    [ProtoInclude(104, typeof(SingleValue))]
    [ProtoInclude(105, typeof(ZeroValue))]
    public abstract class BaseValue
    {
        public abstract float Resolve(Entity entity,ActorAbility ability);
    }
}