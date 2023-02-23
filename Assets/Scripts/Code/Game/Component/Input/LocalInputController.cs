using UnityEngine;

namespace TaoTie
{
    /// <summary>
    /// 玩家的输入
    /// </summary>
    public class LocalInputController : Component, IComponent, IUpdateComponent
    {
        private SkillComponent SkillComponent => Parent.GetComponent<SkillComponent>();
        private AvatarMoveComponent AvatarMoveComponent => Parent.GetComponent<AvatarMoveComponent>();
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
            if (InputManager.Instance.GetKey(GameKeyCode.MoveForward))
            {
                TryMove(Vector3.forward);
            }
            if (InputManager.Instance.GetKey(GameKeyCode.MoveBack))
            {
                TryMove(Vector3.back);
            }
            if (InputManager.Instance.GetKey(GameKeyCode.MoveLeft))
            {
                TryMove(Vector3.left);
            }
            if (InputManager.Instance.GetKey(GameKeyCode.MoveRight))
            {
                TryMove(Vector3.right);
            }
        }

        #endregion

        public void TryDoSkill(int skillID)
        {
            SkillComponent.TryDoSkill(skillID);
        }

        public void TryMove(Vector3 direction)
        {
            AvatarMoveComponent.TryMove(direction);
        }
    }
}