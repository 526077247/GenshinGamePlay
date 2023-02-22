using System.Collections.Generic;

namespace TaoTie
{
    public class MonsterAIInputComponent: Component,IComponent
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
        
        public bool TryDoSkill(int skillId)
        {
            combatComponent.UseSkillImmediately(skillId);
            return true;
        }

        public void TryReleaseSkill()
        {
            combatComponent.ReleaseSkillImmediately();
        }
    }
}