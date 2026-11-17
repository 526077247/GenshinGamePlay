using ProtoBuf;
using UnityEngine;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using TaoTie.Inspector;
#endif

namespace TaoTie
{
    /// <summary>
    /// 状态值比较条件：把 <c>dynDictionary</c> 里的某个状态取出与阈值比较，通过则条件成立。
    ///
    /// <see cref="ByNumericCompare"/> 读的是实体级 <see cref="NumericComponent"/>（所有来源共用一个池），
    /// 这一条读的是「修饰器级 / 施法者级」的状态字典 —— 盾量、剩余次数、充能层数这类
    /// 「每个目标、每次施加各一份」的数据只能用这一条判断。
    ///
    /// <b>左边填 <see cref="BaseValue"/> 而不是写死一个 key</b>：
    /// 单值判断填 <see cref="SpecialValue"/>；要「盾量 / 上限 &gt; 0.3」这类比例判断，
    /// 填 <see cref="OperatorValue"/> 把两个状态做运算即可，不用再加一个专用谓词。
    /// 读写层由 <see cref="SpecialValue.Target"/> 决定，与 <see cref="SetSpecial"/> /
    /// <see cref="AddSpecial"/> 共用 <c>TargetHelper.ResolveSpecialTarget</c>，不会错层。
    ///
    /// <b>基类 <c>Target</c> 字段在这里不参与选层</b>：状态属于 Action 的执行上下文
    /// （ability / modifier），不属于被重定向的实体。它只作为数值求值的宿主实体
    /// （<see cref="FormulaValue"/> / <see cref="NumericValue"/> 用得上）。
    ///
    /// 典型用法：盾量 &gt; 0 才允许破盾反击、免疫次数 == 1 时改播"最后一挡"表现、
    /// 灼烧层数 &gt;= 3 才允许引爆（配合 <see cref="RemoveModifierByTag"/> 消耗层数）。
    /// </summary>
    [ProtoContract][LabelText("状态值比较")]
    public partial class BySpecialCompare : ConfigAbilityPredicate
    {
        [ProtoMember(10)][LabelText("*待比较的状态值")]
        [Tooltip("通常填 SpecialValue；也可以填 OperatorValue 把多个状态做运算（如 Shield/MaxShield）")]
        public BaseValue Value;

        [ProtoMember(11)][LabelText("比较符")]
        [Tooltip("默认 ==（float 用近似比较）。不要给这个字段写非零初始化值，否则配 ==(0) 时会被序列化器当成默认值省略")]
        public CompareMode Compare;

        [ProtoMember(12)][LabelText("比较值")]
        public float Threshold;

        /// <summary>配置对象是全局共享的，漏填只吵一次，避免 Tick 里刷屏。</summary>
        private bool warned;

        public override bool Evaluate(Entity actor, ActorAbility ability, ActorModifier modifier, Entity target)
        {
            if (Value == null)
            {
                Warn(ability, "没填待比较的状态值（Value 为空）");
                return false;
            }

            // 状态层由 Value 自己解析；这里只提供"要在哪个实体上求值"的宿主。
            // 谓词的 Target 常用于 NumericValue / FormulaValue，取不到时退回执行者。
            var ctx = target ?? actor;
            var value = Value.Resolve(ctx, ability, modifier);
            switch (Compare)
            {
                case CompareMode.Equal:
                    return Mathf.Approximately(value, Threshold);
                case CompareMode.NotEqual:
                    return !Mathf.Approximately(value, Threshold);
                case CompareMode.Greater:
                    return value > Threshold;
                case CompareMode.Less:
                    return value < Threshold;
                case CompareMode.LEqual:
                    return value <= Threshold;
                case CompareMode.GEqual:
                    return value >= Threshold;
                default:
                    return false;
            }
        }

        private void Warn(ActorAbility ability, string reason)
        {
            if (warned) return;
            warned = true;
            Log.Error($"[BySpecialCompare] {reason}；Ability = {ability?.Config?.AbilityName}");
        }
    }
}
