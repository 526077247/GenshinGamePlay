using System;
using System.Collections.Generic;
using UnityEngine;

namespace TaoTie
{
    public class AvatarSkillComponent:Component,IComponent,IUpdate
    {
        private CombatComponent combatComponent => parent.GetComponent<CombatComponent>();
        private MoveComponent moveComponent => parent.GetComponent<MoveComponent>();
        public Dictionary<uint, SkillInfo> SkillInfoMap;
        #region IComponent

        public void Init()
        {
            SkillInfoMap = new Dictionary<uint, SkillInfo>();
        }

        public void Destroy()
        {
        }

        public void Update()
        {
            
        }

        #endregion
        
        public void TryDoSkill(int skillId)
        {
            if(combatComponent==null) return;
            //todo: 冷却消耗等判断
            //if()
            {
                combatComponent.SelectAttackTarget(true);
                var target = combatComponent.GetAttackTarget();
                if (target is Unit unit)
                {
                    moveComponent.ForceLookAt(unit.Position);
                }

                combatComponent.UseSkillImmediately(skillId);
            }
        }
    }
}