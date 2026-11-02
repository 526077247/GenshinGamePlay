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
    /// 数值比较条件：Target 的某个数值与阈值比较，通过则条件成立。
    ///
    /// 有了它，「韧性归零时伤害翻倍」「血量低于 30% 才触发处决」这类条件
    /// 就能写在任意 <see cref="ConfigAbilityAction.Predicate"/> 上，不需要再新增专用 Action。
    /// </summary>
    [ProtoContract][LabelText("数值比较")]
    public partial class ByNumericCompare : ConfigAbilityPredicate
    {
        [ProtoMember(10)][LabelText("*数值类型")]
        [Tooltip("填 NumericType 的 id，如 1012(EN)、1001(Hp)")]
        public int NumericType;

        [ProtoMember(11)][LabelText("比较符")]
        [Tooltip("默认 ==。不要给这个字段写非零初始化值，否则配 ==(0) 时会被序列化器当成默认值省略")]
        public CompareMode Compare;

        [ProtoMember(12)][LabelText("比较值")]
        public float Value;

        public override bool Evaluate(Entity actor, ActorAbility ability, ActorModifier modifier, Entity target)
        {
            using var entities = TargetHelper.ResolveTarget(actor, ability, modifier, target, Target);
            if (entities.Count == 0) return false;

            var numD = entities[0].GetComponent<NumericComponent>();
            if (numD == null) return false;
            var value = numD.GetAsFloat(NumericType);
            switch (Compare)
            {
                case CompareMode.Equal:
                    return Mathf.Approximately(value, Value);
                case CompareMode.NotEqual:
                    return !Mathf.Approximately(value, Value);
                case CompareMode.Greater:
                    return value > Value;
                case CompareMode.Less:
                    return value < Value;
                case CompareMode.LEqual:
                    return value <= Value;
                case CompareMode.GEqual:
                    return value >= Value;
                default:
                    return false;
            }
        }
    }
}
