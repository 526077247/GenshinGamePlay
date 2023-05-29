using System;
using UnityEngine;

namespace TaoTie
{
    public class CombatComponent : Component, IComponent
    {
        private FsmComponent fsm => parent.GetComponent<FsmComponent>();
        protected AttackTarget attackTarget;
        public bool IsInCombat;
        public virtual void Init()
        {
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
        /// 开启或关闭hitBox
        /// </summary>
        /// <param name="hitBox"></param>
        /// <param name="enable"></param>
        public async ETTask EnableHitBox(string hitBox, bool enable)
        {
            GameObjectHolderComponent ghc = parent.GetComponent<GameObjectHolderComponent>();
            await ghc.WaitLoadGameObjectOver();
            if(ghc.IsDispose) return;
            ghc.GetCollectorObj<GameObject>(hitBox)?.SetActive(enable);
        }
        
        /// <summary>
        /// 开启或关闭Renderer
        /// </summary>
        /// <param name="enable"></param>
        public async ETTask EnableRenderer(bool enable)
        {
            GameObjectHolderComponent ghc = parent.GetComponent<GameObjectHolderComponent>();
            CoroutineLock coroutineLock = null;
            try
            {
                coroutineLock = await CoroutineLockManager.Instance.Wait(CoroutineLockType.EnableObjView, parent.Id);
                await ghc.WaitLoadGameObjectOver();
                if(ghc.IsDispose) return;
                var renders = ghc.EntityView.GetComponentsInChildren<Renderer>();
                for (int i = 0; i < renders.Length; i++)
                {
                    renders[i].enabled = enable;
                }
            }
            finally
            {
                coroutineLock?.Dispose();
            }
            
        }
    }
}