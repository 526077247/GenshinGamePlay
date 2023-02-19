using System;
using System.Collections.Generic;
using UnityEngine;

namespace TaoTie
{
    public class SkillComponent:Component,IComponent,IUpdateComponent
    {
        private CombatComponent combatComponent => Parent.GetComponent<CombatComponent>();
        public Dictionary<uint, SkillInfo> skillInfoMap;
        #region IComponent

        public void Init()
        {
            skillInfoMap = new Dictionary<uint, SkillInfo>();
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
            //todo: 冷却消耗等判断
            combatComponent.UseSkillImmediately(skillId);
        }
    }
}