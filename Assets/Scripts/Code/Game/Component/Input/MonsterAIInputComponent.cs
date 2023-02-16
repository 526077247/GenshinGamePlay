using System.Collections.Generic;

namespace TaoTie
{
    public class MonsterAIInputComponent:  Component,IComponent
    {
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
            
        }
    }
}