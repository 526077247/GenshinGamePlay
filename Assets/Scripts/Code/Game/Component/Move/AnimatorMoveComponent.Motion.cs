using UnityEngine;

namespace TaoTie
{
    public partial class AnimatorMoveComponent
    {
        private FsmComponent fsm => GetComponent<FsmComponent>();
        private ORCAAgentComponent orcaAgent => GetComponent<ORCAAgentComponent>();
        private long lastAnimatorMoveTime;
        private void OnAnimatorMove()
        {
            var timeNow = GameTimerManager.Instance.GetTimeNow();
            var animator = GetComponent<UnitModelComponent>()?.GetAnimator();
            if (animator != null)
            {
                if (CharacterInput.HitImpulse.sqrMagnitude > 0)
                {
                    var deltaTime = timeNow - lastAnimatorMoveTime;
                    var oldHitImpulse = CharacterInput.HitImpulse;
                    var newHitImpulse = CharacterInput.HitImpulse - CharacterInput.HitImpulse.normalized * deltaTime / 100f;
                    if (Vector3.Dot(oldHitImpulse, newHitImpulse) < 0.5f)
                    {
                        CharacterInput.HitImpulse = Vector3.zero;
                    }
                    else
                    {
                        CharacterInput.HitImpulse = newHitImpulse;
                    }
                    CharacterInput.Velocity = Quaternion.Inverse(transform.rotation) * CharacterInput.HitImpulse * 5;
                }
                else
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
            }
            else
            {
                orcaAgent?.SetVelocity(Vector3.zero, CharacterInput.SpeedScale);
            }

            lastAnimatorMoveTime = timeNow;
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