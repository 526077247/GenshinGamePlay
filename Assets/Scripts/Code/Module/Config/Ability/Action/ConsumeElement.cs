using ProtoBuf;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using TaoTie.Inspector;
#endif
using UnityEngine;

namespace TaoTie
{
    /// <summary>
    /// 扣掉目标身上的元素量（元素反应的"支付"动作）。
    ///
    /// <b>本 Action 只扣量，不产生任何后果</b>：扣完是加伤害、是打一段反应伤害、还是给个易伤，
    /// 全部由同一条 Actions 里后面的 Action 决定 —— 这就是"框架层不做自动反应"的落点。
    ///
    /// 扣除<b>同步生效</b>，所以同一条 Actions 里后面用 <see cref="ElementValue"/> 读到的已经是扣后值；
    /// 反过来，想读"扣之前有多少"必须把读取写在它前面（或提前 SetSpecial 存下来）。
    ///
    /// 时序：挂在受击钩子（BeforeBeAttack / OnBeingHit）里最自然 ——
    /// 那时 <see cref="AttackHelper.DamageClose"/> 还没附着本次攻击的新元素，读到的是"被打之前"的元素，
    /// 不会自己吃掉自己刚挂上的元素。
    /// </summary>
    [ProtoContract]
    [LabelText("消耗元素量")]
    public partial class ConsumeElement : ConfigAbilityAction
    {
        [ProtoMember(10)][LabelText("元素Id")]
        [ShowIf("@" + nameof(FromAttack) + "==false")]
        [Tooltip("要扣的目标元素。0 不会扣任何东西；FromAttack 勾上时本字段被忽略")]
        public int Element;

        [ProtoMember(11)][LabelText("从本次攻击取元素")]
        [Tooltip("勾上则扣「本次这一击的元素」（来自 ConfigAttackProperty.Element），\n" +
                 "适合写在通用受击钩子里、由攻击方决定元素的配置。只能用在攻击上下文（有 AttackResult），\n" +
                 "其余链路会打一条警告并跳过。\n" +
                 "注意：元素组件已在每次附着后自动结算反应，这里只用于「反应之外还要额外扣一点」的场合。")]
        public bool FromAttack;

        [ProtoMember(12)][LabelText("消耗量")]
        [Tooltip("留空 = 全部扣光（最常见的反应写法）。填 OperatorValue 可以做「扣当前的一半」这类按比例消耗")]
        public BaseValue Amount;

        [ProtoMember(13)][LabelText("结果写入状态")]
        [Tooltip("把实际扣掉的元素量写进 AbilitySpecials 的这个 key，供后续 Action 按「真扣了多少」结算伤害；留空不写。\n" +
                 "读回时 SpecialValue 要显式填 Target=Ability")]
        public string ResultKey;

        /// <summary>配置对象全局共享，漏填只吵一次（可能挂在每次命中的 Mixin 上）。</summary>
        private bool warned;

        protected override void Execute(Entity actionExecuter, ActorAbility ability, ActorModifier modifier, Entity target)
        {
            Execute(actionExecuter, ability, modifier, target, null);
        }

        protected override void Execute(Entity actionExecuter, ActorAbility ability, ActorModifier modifier,
                                        Entity target, AttackResult result)
        {
            if (target == null || target.IsDispose) return;

            var element = target.GetComponent<ElementComponent>();
            if (element == null) return; // 目标没有元素体系：常态，不算错误

            // <see cref="BaseValue"/> 允空：留空就是"全扣"，这是最常见写法，不当漏配处理
            float amount = Amount?.Resolve(target, ability, modifier) ?? 0f;
            float consumed = 0f;

            if (FromAttack)
            {
                if (result == null || result.Element <= 0)
                {
                    WarnOnce($"勾了 FromAttack 但本次结算没有元素（result={(result == null ? "null" : "非空")}）");
                    return;
                }
                consumed = element.ConsumeByConfig(result.Element, amount);
            }
            else
            {
                if (Element <= 0)
                {
                    WarnOnce("Element 填 0 且没勾 FromAttack：这条 Action 什么也不会消耗");
                    return;
                }
                consumed = element.ConsumeByConfig(Element, amount);
            }

            if (consumed > 0 && !string.IsNullOrEmpty(ResultKey) && ability != null && !ability.IsDispose)
            {
                ability.SetSpecial(ResultKey, consumed);
            }
        }

        private void WarnOnce(string reason)
        {
            if (warned) return;
            warned = true;
            Log.Warning($"[Element] ConsumeElement {reason}");
        }
    }
}
