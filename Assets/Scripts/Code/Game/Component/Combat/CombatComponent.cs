using System;
using UnityEngine;

namespace TaoTie
{
    public class CombatComponent : Component, IComponent<ConfigCombat>
    {
        private FsmComponent fsm => parent.GetComponent<FsmComponent>();
        protected AttackTarget attackTarget;
        public bool IsInCombat;
        public DieStateFlag DieStateFlag;
        private ConfigCombat config;
        public virtual void Init(ConfigCombat config)
        {
            this.config = config;
            attackTarget = new AttackTarget();
        }

        public virtual void Destroy()
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
        /// 受到伤害后
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
            Messager.Instance.Broadcast(0,MessageId.ShowDamageText,result);
        }

        /// <summary>
        /// 立刻使用技能
        /// </summary>
        /// <param name="skillId"></param>
        public void UseSkillImmediately(int skillId)
        {
            fsm.SetData(FSMConst.UseSkill, true);
            fsm.SetData(FSMConst.SkillId, skillId);
        }
        /// <summary>
        /// 立即停止使用技能
        /// </summary>
        public void ReleaseSkillImmediately()
        {
            fsm.SetData(FSMConst.UseSkill, false);
            fsm.SetData(FSMConst.SkillId, 0);
        }

        /// <summary>
        /// 被击杀
        /// </summary>
        /// <param name="killerID"></param>
        /// <param name="dieType"></param>
        public void DoKill(long killerID, DieStateFlag dieType)
        {
            DieStateFlag = dieType;
            var myId = Id;
            Messager.Instance.Broadcast(myId, MessageId.OnBeKill, config?.Die, dieType);
            Dispose();
            if(killerID != myId)
                Messager.Instance.Broadcast(killerID, MessageId.OnKill, myId);
        }
    }
}