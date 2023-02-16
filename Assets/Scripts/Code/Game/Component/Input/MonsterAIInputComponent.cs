using System.Collections.Generic;

namespace TaoTie
{
    public class MonsterAIInputComponent:  Component,IComponent
    {
        private CombatComponent combatComponent => Parent.GetComponent<CombatComponent>();
        private Dictionary<int, bool> animatorParamCache;
        public ControlData controlData;
        #region IComponent

        public void Init()
        {
            controlData = ControlData.Create();
        }

        public void Destroy()
        {
            controlData.Dispose();
            controlData = null;
        }

        #endregion
        
        public void TryDoSkill(int skillId)
        {
            combatComponent.SetFsmSkillParam(skillId);
        }
    }
}