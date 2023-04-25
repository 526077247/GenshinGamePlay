using UnityEngine;

namespace TaoTie
{
    public class AvatarMoveComponent : Component, IComponent
    {
        private bool canMove = true;
        private bool canTurn = true;
        private FsmComponent FsmComponent => parent.GetComponent<FsmComponent>();
        private NumericComponent NumericComponent => parent.GetComponent<NumericComponent>();
        public void Init()
        {
            canMove = FsmComponent.DefaultFsm.currentState.CanMove;
            canTurn = FsmComponent.DefaultFsm.currentState.CanTurn;
            Messager.Instance.AddListener<bool>(Id, MessageId.SetCanMove, SetCanMove);
            Messager.Instance.AddListener<bool>(Id, MessageId.SetCanTurn, SetCanTurn);
        }

        public void Destroy()
        {
            Messager.Instance.RemoveListener<bool>(Id, MessageId.SetCanMove, SetCanMove);
            Messager.Instance.RemoveListener<bool>(Id, MessageId.SetCanTurn, SetCanTurn);
        }

        public void TryMove(Vector3 direction)
        {
            if (canMove)
            {
                GetParent<Unit>().Position += (GameTimerManager.Instance.GetDeltaTime() / 1000f) *
                                              NumericComponent.GetAsFloat(NumericType.Speed) * direction;
                if (direction != Vector3.zero)
                {
                    MoveStart();
                }
                else
                {
                    MoveStop();
                }
            }
            if (canTurn)
            {
                if (GetParent<Unit>().IsTurn && direction.x > 0)
                {
                    GetParent<Unit>().IsTurn = false;
                }

                if (!GetParent<Unit>().IsTurn && direction.x < 0)
                {
                    GetParent<Unit>().IsTurn = true;
                }
            }
        }

        private void SetCanMove(bool canMove)
        {
            this.canMove = canMove;
        }

        private void SetCanTurn(bool canTurn)
        {
            this.canTurn = canTurn;
        }

        private void MoveStart()
        {
            FsmComponent.SetData(FSMConst.MotionFlag, 1);
        }

        private void MoveStop()
        {
            FsmComponent.SetData(FSMConst.MotionFlag, 0);
        }
    }
}
