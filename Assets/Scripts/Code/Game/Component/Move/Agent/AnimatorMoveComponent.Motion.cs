using UnityEngine;

namespace TaoTie
{
    public partial class AnimatorMoveComponent
    {
        // 状态动画速率（RVO 减速适配结果），由 UpdateInternal 统一写入 animator.speed
        private float currentPlaySpeed = 1f;

        /// <summary>
        /// Animator 根运动采样（每个动画帧回调，经 AnimatorMoveBridge 转发）：
        /// 读取 animator.velocity，经 ORCA 偏好速度/减速适配后写入 CharacterInput.Velocity（局部系）。
        /// 击退期间不采样动画位移，位移由 FixedUpdate 的击退通道驱动。
        /// </summary>
        private void OnAnimatorMove()
        {
            if (animator == null || view == null || CharacterInput == null) return;
            var state = fsm?.DefaultFsm?.CurrentState;
            if (state == null) return;

            if (CharacterInput.HitImpulse.sqrMagnitude > 0f)
            {
                CharacterInput.Velocity = Vector3.zero;
                CharacterInput.Jump = state.IsJump;
                PinViewRotation();
                return;
            }

            float playSpeed = state.EffectBySpeed ? CharacterInput.SpeedScale : 1f;
            var animVelocity = animator.velocity;
            var preVelocity = animVelocity * animator.speed;
            // 局部系换算基准必须是 SceneEntity.Rotation（水平 yaw），不能是 view.rotation：
            // view.rotation 此刻带着贴坡倾斜，与 GetAnimatorHorizontalVelocity 的 rot*local 不成互逆，
            // 坡上往返换算会把速度拧出方向偏差；倾斜只用于渲染，不参与位移。
            var localFrame = SceneEntity.Rotation;
            var velocity = Quaternion.Inverse(localFrame) * animVelocity;
            orcaAgent?.SetVelocity(preVelocity, preVelocity.magnitude);
            if (orcaAgent != null && state.EffectBySpeed)
            {
                var agentVelocity = orcaAgent.GetVelocity();
                if (agentVelocity.magnitude < animVelocity.magnitude)
                    playSpeed *= agentVelocity.magnitude / animVelocity.magnitude;
                velocity = Quaternion.Inverse(localFrame) * agentVelocity;
            }
            currentPlaySpeed = playSpeed;
            CharacterInput.Velocity = velocity;

            // 必须在上面用带动差的 view.rotation 算完 CharacterInput.Velocity（动画局部系）之后调用
            PinViewRotation();
            CharacterInput.Jump = state.IsJump;
        }

        /// <summary>
        /// 把 EntityView 旋转钉回 SceneEntity.Rotation，剔除动画根旋转对视图朝向的污染。
        /// Generic 资产（如 Fox）的动画带 ~180° 根旋转，Unity 在 OnAnimatorMove 之前已把它应用到
        /// animator.transform（=EntityView），而视图旋转同步（OnChangeRotation）只在实体旋转变化时
        /// 推送，直行时 ent 恒定、偏差永不纠正，模型背朝移动方向表现为倒退跑（位置方向是对的）。
        /// 位移侧不受影响：FixedUpdate 的 GetAnimatorHorizontalVelocity 只读 CharacterInput.Velocity
        /// 与 SceneEntity.Rotation，不读 view.rotation；KeQing 等根旋转≈0 的资产本钉住为无操作。
        /// 贴坡倾斜在此叠加（GroundTilt × yaw）：每渲染帧现算，yaw 零延迟跟随转向，
        /// 倾斜跟随 CheckGround 的法线平滑结果——四足在坡上即时贴坡，且不会污染逻辑朝向。
        /// 倾斜的首次落盘在 FixedUpdate 末尾（SyncViewTilt），这里只负责根运动之后再钉一次。
        /// </summary>
        private void PinViewRotation()
        {
            SyncViewTilt();
        }
    }
}
