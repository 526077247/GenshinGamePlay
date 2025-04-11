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
                float playSpeed = fsm.DefaultFsm.CurrentState.EffectBySpeed ? CharacterInput.SpeedScale : 1f;
                var animVelocity = animator.velocity;
                var preVelocity = animVelocity * animator.speed;
                
                var velocity = Quaternion.Inverse(transform.rotation) * animVelocity;
                orcaAgent?.SetVelocity(preVelocity, preVelocity.magnitude);
                if (orcaAgent != null && fsm.DefaultFsm.CurrentState.EffectBySpeed)
                {
                    var agentVelocity = orcaAgent.GetVelocity();
                    if (agentVelocity.magnitude < animator.velocity.magnitude)
                    {
                        playSpeed *= agentVelocity.magnitude / animator.velocity.magnitude;
                    }
                    velocity = Quaternion.Inverse(transform.rotation) * agentVelocity;
                }
                var timeScale = GameTimerManager.Instance.GetTimeScale();
               
                animator.speed = timeScale * playSpeed;
                CharacterInput.Velocity = velocity;
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