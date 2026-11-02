using ProtoBuf;
#if UNITY_EDITOR
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using TaoTie.Inspector;
#endif
#endif
namespace TaoTie
{
    /// <summary>
    /// 把某个数值直接设为指定值（不是加减）。
    ///
    /// 与 <see cref="AddNumericValue"/> 的唯一区别就是"设为"而非"加"，
    /// 但这决定了它能不能安全地做"回满"：
    /// 加法要预先知道当前值才能算出该加多少，而当前值可能被别的来源改过，
    /// 加多就顶穿上限（AddNumericValue 里没有上限保护）。配合
    /// <see cref="NumericValue"/> 读上限属性，就能写成"把当前值设为上限"，
    /// 不管之前是多少都不会越界。
    ///
    /// <see cref="Key"/> 必须填 <b>base 段</b>（如 ENBase=10121）：
    /// <see cref="NumericComponent.Set(int,float,bool)"/> 不允许直接写最终段，写最终段会被
    /// 强制改成 base 段并打一条 Error。
    /// </summary>
    [ProtoContract]
    public partial class SetNumericValue : ConfigAbilityAction
    {
        [ProtoMember(10)]
#if UNITY_EDITOR
        [ValueDropdown("@"+nameof(OdinDropdownHelper)+"."+nameof(OdinDropdownHelper.GetNumericTypeId)+"()")]
#endif
        public int Key;
        [ProtoMember(11, IsRequired = true)]
        public BaseValue Value = new ZeroValue();

        protected override void Execute(Entity actionExecuter, ActorAbility ability, ActorModifier modifier, Entity target)
        {
            var numC = target.GetComponent<NumericComponent>();
            if (numC != null)
            {
                numC.Set(Key, Value.Resolve(target, ability));
            }
        }
    }
}
