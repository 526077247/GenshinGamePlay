using UnityEngine;

namespace TaoTie
{
    /// <summary>
    /// AI输入
    /// </summary>
    public class AIInputController: Component, IComponent
    {
        private MoveComponent moveComponent => parent.GetComponent<MoveComponent>();
        private FsmComponent fsm => parent.GetComponent<FsmComponent>();
        private bool canMove = true;
        private bool canTurn = true;
        
        #region IComponent

        public void Init()
        {
            canMove = fsm.DefaultFsm.CurrentState.CanMove;
            canTurn = fsm.DefaultFsm.CurrentState.CanTurn;
            Messager.Instance.AddListener<bool>(Id, MessageId.SetCanMove, SetCanMove);
            Messager.Instance.AddListener<bool>(Id, MessageId.SetCanTurn, SetCanTurn);
        }

        public void Destroy()
        {
            Messager.Instance.RemoveListener<bool>(Id, MessageId.SetCanMove, SetCanMove);
            Messager.Instance.RemoveListener<bool>(Id, MessageId.SetCanTurn, SetCanTurn);
        }

        
        #endregion
        
        public void TryMove(Vector3 direction, MotionFlag mFlag = MotionFlag.Run, MotionDirection mDirection = MotionDirection.Forward)
        {
            if (direction == Vector3.zero)
            {
                fsm.SetData(FSMConst.MotionFlag, 0);
                fsm.SetData(FSMConst.MotionDirection, 0);
            }
            else
            {
                fsm.SetData(FSMConst.MotionFlag, (int)mFlag);
                fsm.SetData(FSMConst.MotionDirection, (int)mDirection);
            }
            if (canTurn)
                moveComponent.CharacterInput.Direction = direction;
            else
                moveComponent.CharacterInput.Direction = Vector3.zero;
        }
        
        private void SetCanMove(bool canMove)
        {
            if(IsDispose) return;
            this.canMove = canMove;
        }

        private void SetCanTurn(bool canTurn)
        {
            if(IsDispose) return;
            this.canTurn = canTurn;
        }
    }
}