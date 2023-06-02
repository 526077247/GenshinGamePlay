
using UnityEngine;

namespace TaoTie
{
    /// <summary>
    /// 玩家的输入
    /// </summary>
    public class LocalInputController : Component, IComponent, IUpdate
    {
        private AvatarSkillComponent AvatarSkillComponent => parent.GetComponent<AvatarSkillComponent>();
        private MoveComponent MoveComponent => parent.GetComponent<MoveComponent>();
        
        #region IComponent

        public void Init()
        {
            
        }

        public void Destroy()
        {

        }

        public void Update()
        {
            if (InputManager.Instance.GetKey(GameKeyCode.NormalAttack))
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
            AvatarSkillComponent.TryDoSkill(skillID);
        }

        public void TryMove(Vector3 direction)
        {
            MoveComponent.TryMove(direction, MotionFlag.Run);
        }
    }
}