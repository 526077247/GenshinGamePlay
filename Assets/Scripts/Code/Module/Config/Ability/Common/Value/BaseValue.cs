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
    [ProtoInclude(106, typeof(TagStackValue))]
    public abstract class BaseValue
    {
        /// <summary>
        /// 求值
        /// <paramref name="modifier"/> 允许为 null —— 那表示"当前求值不属于任何修饰器"
        /// （技能本体的 ability 级 mixin、范围检测、子弹轨迹等），
        /// 而不是"层数是 0"。两者语义不同，别在实现里混为一谈。
        /// </summary>
        public abstract float Resolve(Entity entity, ActorAbility ability, ActorModifier modifier = null);
    }
}