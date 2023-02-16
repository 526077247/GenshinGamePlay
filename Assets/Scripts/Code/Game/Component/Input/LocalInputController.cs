using UnityEngine;

namespace TaoTie
{
    public class LocalInputController: Component,IComponent,IUpdateComponent
    {
        private SkillComponent SkillComponent => Parent.GetComponent<SkillComponent>();
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

        public void Update()
        {
            if (InputManager.Instance.GetKey(GameKeyCode.Skill))
            {
                TryDoSkill(1001);
            }
        }

        #endregion
        
        public void TryDoSkill(int skillID)
        {
            SkillComponent.TryDoSkill(skillID);
        }
    }
}