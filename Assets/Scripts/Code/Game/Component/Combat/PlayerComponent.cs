using System;
using UnityEngine;

namespace TaoTie
{
    public class PlayerComponent:Component,IComponent,IUpdateComponent
    {
        private CombatComponent combatComponent => Parent.GetComponent<CombatComponent>();
        #region IComponent

        public void Init()
        {
        }

        public void Destroy()
        {
        }
        
        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                combatComponent.UseSkill(1001);
            }
        }

        #endregion
    }
}