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
            beforeAttack = null;
            beforeBeAttack = null;
            afterAttack = null;
            afterBeAttack = null;
        }

        /// <summary>
        /// 造成伤害前
        /// </summary>
        public event Action<AttackResult, CombatComponent> beforeAttack;

        /// <summary>
        /// 受到伤害前
        /// </summary>
        public event Action<AttackResult, CombatComponent> beforeBeAttack;

        /// <summary>
        /// 造成伤害后
        /// </summary>
        public event Action<AttackResult, CombatComponent> afterAttack;

        /// <summary>
        /// 造成伤害后
        /// </summary>
        public event Action<AttackResult, CombatComponent> afterBeAttack;

        /// <summary>
        /// 造成伤害前
        /// </summary>
        public void BeforeAttack(AttackResult result, CombatComponent other)
        {
            beforeAttack?.Invoke(result, other);
        }

        /// <summary>
        /// 受到伤害前
        /// </summary>
        public void BeforeBeAttack(AttackResult result, CombatComponent other)
        {
            beforeBeAttack?.Invoke(result, other);
        }

        /// <summary>
        /// 造成伤害后
        /// </summary>
        public void AfterAttack(AttackResult result, CombatComponent other)
        {
            afterAttack?.Invoke(result, other);
        }

        /// <summary>
        /// 受到伤害后
        /// </summary>
        public void AfterBeAttack(AttackResult result, CombatComponent other)
        {
            afterBeAttack?.Invoke(result, other);
        }
    }
}