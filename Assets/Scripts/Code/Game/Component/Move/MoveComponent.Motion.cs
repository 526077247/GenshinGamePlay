using UnityEngine;

namespace TaoTie
{
    public partial class MoveComponent
    {
        private FsmComponent fsm => GetComponent<FsmComponent>();
        private void OnAnimatorMove()
        {
            var animator = GetComponent<GameObjectHolderComponent>()?.Animator;
            if (animator != null)
            {
                CharacterInput.Speed = Quaternion.Inverse(transform.rotation) * animator.velocity;
                // animator.ApplyBuiltinRootMotion();
                CharacterInput.Jump = fsm.DefaultFsm.CurrentState.IsJump;
                fsm.SetData(FSMConst.Speed, CharacterInput.GetVerticalMovementInput());
                fsm.SetData(FSMConst.Land, mover.IsGrounded());
            }
        }

        private void OnJump(Vector3 v)
        {
			Log.Info("OnJump");
            fsm.SetData(FSMConst.Land, false);
        }

        private void OnLand(Vector3 v)
        {
            Log.Info("OnLand");
            fsm.SetData(FSMConst.Land, true);
        }
    }
}