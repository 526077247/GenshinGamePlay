using System;
using System.Collections.Generic;
using UnityEngine;

namespace TaoTie
{
    public class SkillComponent:Component,IComponent,IUpdateComponent
    {
        private CombatComponent combatComponent => parent.GetComponent<CombatComponent>();
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
            //todo: 冷却消耗等判断
            combatComponent.UseSkillImmediately(skillId);
        }
    }
}