using UnityEngine;

namespace TaoTie
{
    /// <summary>
    /// 玩家的输入
    /// </summary>
    public class LocalInputController : Component, IComponent, IUpdateComponent
    {
        private SkillComponent SkillComponent => parent.GetComponent<SkillComponent>();
        private AvatarMoveComponent AvatarMoveComponent => parent.GetComponent<AvatarMoveComponent>();
        public ControlData ControlData;
        #region IComponent

        public void Init()
        {
            ControlData = ControlData.Create();
        }

        public void Destroy()
        {
            ControlData.Dispose();
            ControlData = null;
        }

        public void Update()
        {
            if (InputManager.Instance.GetKey(GameKeyCode.Skill))
            {
                TryDoSkill(1001);
            }

            //移动
            Vector3 direction = Vector3.zero;
            if (InputManager.Instance.GetKey(GameKeyCode.MoveForward))
            {
                direction += Vector3.forward;
            }
            if (InputManager.Instance.GetKey(GameKeyCode.MoveBack))
            {
                direction += Vector3.back;
            }
            if (InputManager.Instance.GetKey(GameKeyCode.MoveLeft))
            {
                direction += Vector3.left;
            }
            if (InputManager.Instance.GetKey(GameKeyCode.MoveRight))
            {
                direction += Vector3.right;
            }
            this.TryMove(Vector3.Normalize(direction));
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