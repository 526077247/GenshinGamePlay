using UnityEngine;

namespace TaoTie
{
    public class AvatarMoveComponent : Component, IComponent
    {
        private bool canMove = true;
        private bool canTurn = true;
        private FsmComponent FsmComponent => Parent.GetComponent<FsmComponent>();
        private NumericComponent NumericComponent => Parent.GetComponent<NumericComponent>();
        public void Init()
        {
            canMove = FsmComponent.defaultFsm.currentState.CanMove;
            canTurn = FsmComponent.defaultFsm.currentState.CanTurn;
            Messager.Instance.AddListener<bool>(Id, MessageId.SetCanMove,SetCanMove);
            Messager.Instance.AddListener<bool>(Id, MessageId.SetCanTurn,SetCanTurn);
        }

        public void Destroy()
        {
            Messager.Instance.RemoveListener<bool>(Id, MessageId.SetCanMove,SetCanMove);
            Messager.Instance.RemoveListener<bool>(Id, MessageId.SetCanTurn,SetCanTurn);
        }

        public void TryMove(Vector3 direction)
        {
            if (canMove)
            {
                GetParent<Unit>().Position += (GameTimerManager.Instance.GetDeltaTime() / 1000f) *
                                              NumericComponent.GetAsFloat(NumericType.Speed) * direction;
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
    }
}
