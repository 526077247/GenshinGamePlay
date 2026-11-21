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
    /// 真吸血：按本次命中的伤害回血给施法者。
    /// 必须挂在攻击钩子（AfterAttack 等）；非攻击钩子里 result=null 会报错而不是静默吞掉。
    /// 默认 Targetting=Caster，回到 ability 持有者而不是受击怪物。
    /// 回血键用 HpBase（与扣血同一键），上限 clamp 到 MaxHp（AddNumericValue 的 //todo 不做上限）。
    /// </summary>
    [ProtoContract]
    [Tooltip("真吸血：按本次伤害比例回血给施法者")]
    public partial class LifeSteal : ConfigAbilityAction
    {
        /// <summary>吸血比例，可填 SingleValue / FormulaValue（公式能读 Hp/MaxHp/ATK）。</summary>
        [ProtoMember(11)]
        public BaseValue Ratio = new SingleValue(0.2f);

        /// <summary>true=按实际掉血(FinalRealDamage-AbsorbedDamage，打盾不吸)；false=按全额。</summary>
        [ProtoMember(12)]
        [LabelText("按实际掉血(扣盾后)")]
        public bool UseHpDamage = true;

        /// <summary>单次回血上限，0=不限制。用于群攻多目标防超量。</summary>
        [ProtoMember(13)]
        [LabelText("单次回血上限(0=不限)")]
        public int MaxHealPerHit;

        public LifeSteal()
        {
            Targetting = AbilityTargetting.Caster;
        }

        // 4 参是 abstract，必须实现：非攻击链路掉到这里，报错而不是静默
        protected override void Execute(Entity actionExecuter, ActorAbility ability, ActorModifier modifier, Entity target)
        {
            Execute(actionExecuter, ability, modifier, target, null);
        }

        protected override void Execute(Entity actionExecuter, ActorAbility ability, ActorModifier modifier,
            Entity target, AttackResult result)
        {
            if (result == null)
            {
                Log.Error($"LifeSteal 只能用于攻击钩子，当前上下文无结算中的攻击：{ability?.Config?.AbilityName}");
                return;
            }
            if (!result.IsEffective) return;

            var numC = target?.GetComponent<NumericComponent>();
            if (numC == null) return;

            int dmg = result.FinalRealDamage - (UseHpDamage ? result.AbsorbedDamage : 0);
            if (dmg <= 0) return;

            float ratio = Ratio?.Resolve(target, ability, modifier) ?? 0f;
            if (ratio <= 0f) return;

            float heal = dmg * ratio;
            if (MaxHealPerHit > 0) heal = Mathf.Min(heal, MaxHealPerHit);

            float now = numC.GetAsFloat(NumericType.HpBase);
            float max = numC.GetAsFloat(NumericType.MaxHp);
            float next = Mathf.Min(now + heal, max);
            if (next > now) numC.Set(NumericType.HpBase, next);
        }
    }
}
