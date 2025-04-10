using UnityEngine;

namespace TaoTie
{
    public partial class MoveComponent
    {
        private FsmComponent fsm => GetComponent<FsmComponent>();
        private ORCAAgentComponent orcaAgent => GetComponent<ORCAAgentComponent>();
        private void OnAnimatorMove()
        {
            var animator = GetComponent<UnitModelComponent>()?.GetAnimator();
            if (animator != null)
            {
                var timeScale = GameTimerManager.Instance.GetTimeScale();
                float playSpeed = fsm.DefaultFsm.CurrentState.EffectBySpeed ? CharacterInput.SpeedScale : 1f;
                var velocity = Quaternion.Inverse(transform.rotation) * animator.velocity;
                animator.speed = timeScale * playSpeed;
                CharacterInput.Velocity = velocity;
                orcaAgent?.SetVelocity(velocity, velocity.magnitude);
                // animator.ApplyBuiltinRootMotion();
                CharacterInput.Jump = fsm.DefaultFsm.CurrentState.IsJump;
                fsm.SetData(FSMConst.Speed, CharacterInput.GetVerticalMovementInput());
                fsm.SetData(FSMConst.Land, mover.IsGrounded());
            }
            else
            {
                orcaAgent?.SetVelocity(Vector3.zero, CharacterInput.SpeedScale);
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