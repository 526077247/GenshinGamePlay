using System;

namespace TaoTie
{
    public class CombatComponent : Component, IComponent
    {
        public void Init()
        {
            
        }

        public void Destroy()
        {
            beforeMakeDamage = null;
            beforeApplyDamage = null;
            afterMakeDamage = null;
            afterApplyDamage = null;
        }

        /// <summary>
        /// 造成伤害前
        /// </summary>
        public event Action<AttackResult, CombatComponent> beforeMakeDamage;

        /// <summary>
        /// 受到伤害前
        /// </summary>
        public event Action<AttackResult, CombatComponent> beforeApplyDamage;

        /// <summary>
        /// 造成伤害后
        /// </summary>
        public event Action<AttackResult, CombatComponent> afterMakeDamage;

        /// <summary>
        /// 造成伤害后
        /// </summary>
        public event Action<AttackResult, CombatComponent> afterApplyDamage;

        /// <summary>
        /// 造成伤害前
        /// </summary>
        public void BeforeMakeDamage(AttackResult result, CombatComponent other)
        {
            beforeMakeDamage?.Invoke(result, other);
        }

        /// <summary>
        /// 受到伤害前
        /// </summary>
        public void BeforeApplyDamage(AttackResult result, CombatComponent other)
        {
            if (!result.IgnoreAttackerProperty)
            {
                var numC = Parent.GetComponent<NumericComponent>();
                if (numC != null)
                {
                    //todo:等级、防御、抵抗计算
                }
            }

            beforeApplyDamage?.Invoke(result, other);
        }

        /// <summary>
        /// 造成伤害后
        /// </summary>
        public void AfterMakeDamage(AttackResult result, CombatComponent other)
        {
            afterMakeDamage?.Invoke(result, other);
        }

        /// <summary>
        /// 受到伤害后
        /// </summary>
        public void AfterApplyDamage(AttackResult result, CombatComponent other)
        {
            afterApplyDamage?.Invoke(result, other);
        }
    }
}