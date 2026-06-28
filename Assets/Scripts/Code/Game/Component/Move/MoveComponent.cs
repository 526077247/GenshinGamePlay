using UnityEngine;

namespace TaoTie
{
    public abstract partial class MoveComponent: Component,IComponentDestroy,IUpdate
    {
        protected SceneEntity SceneEntity => parent as SceneEntity;
        
        public ConfigMoveAgent Config { get; private set; }
        public MoveInput CharacterInput{ get; private set; }
        /// <summary>
        /// 是否使用动画移动
        /// </summary>
        public abstract bool useAnimMove { get; }

        private MoveStrategy strategy;
        protected void Init(ConfigMoveAgent configMove)
        {
            Config = configMove;
            CharacterInput = new MoveInput();
        }

        public virtual void Destroy()
        {
            strategy?.Dispose();
            strategy = null;
            CharacterInput = null;
            Config = null;
        }

        /// <summary>
        /// 强制朝向目标
        /// </summary>
        /// <param name="target"></param>
        public virtual void ForceLookAt(Vector3 target)
        {
            Vector3 dir = target - SceneEntity.Position;
            dir.y = 0;
            SceneEntity.Rotation = Quaternion.LookRotation(dir, Vector3.up);
        }
        /// <summary>
        /// 强制朝向方向
        /// </summary>
        /// <param name="dir"></param>
        public virtual void ForceLookTo(Vector3 dir)
        {
            if (CameraManager.Instance?.MainCamera() == null) return;
            var lookDir = CameraManager.Instance.MainCamera().transform.rotation * dir;
            lookDir.y = 0;
            if (lookDir.sqrMagnitude < 0.0001f) return;
            SceneEntity.Rotation = Quaternion.LookRotation(lookDir, Vector3.up);
        }

        public void Update()
        {
            if (strategy != null)
            {
                strategy?.Update();
            }

            if (strategy?.OverrideUpdate != true)
            {
                UpdateInternal();
            }
        }
        
        protected abstract void UpdateInternal();

        public void ChangeStrategy(ConfigMoveStrategy configMoveStrategy)
        {
            strategy?.Dispose();
            strategy = null;
            if (configMoveStrategy != null)
            {
                strategy = MoveSystem.Instance.CreateMoveStrategy(this, configMoveStrategy);
            }
        }
        
        public void ChangeStrategy<P1>(ConfigMoveStrategy configMoveStrategy,P1 p1)
        {
            strategy?.Dispose();
            strategy = null;
            if (configMoveStrategy != null)
            {
                strategy = MoveSystem.Instance.CreateMoveStrategy(this, configMoveStrategy, p1);
            }
        }

        /// <summary>
        /// 根据输入计算期望朝向
        /// </summary>
        protected Vector3 CalculateLookDirection()
        {
            if (CharacterInput == null || CharacterInput.FaceDirection == Vector3.zero)
                return Vector3.zero;

            Vector3 v = Vector3.zero;
            var up = SceneEntity.Up;
            var faceRight = Quaternion.Euler(0, 90, 0) * CharacterInput.FaceDirection;
            v += Vector3.ProjectOnPlane(faceRight, up).normalized * CharacterInput.Direction.x;
            v += Vector3.ProjectOnPlane(CharacterInput.FaceDirection, up).normalized * CharacterInput.Direction.z;

            v.Normalize();
            return v;
        }

        /// <summary>
        /// 根据输入和转向速度执行旋转
        /// </summary>
        protected void HandleRotation(float deltaTime)
        {
            if (CharacterInput == null || CharacterInput.RotateSpeed <= 0) return;
            var lookDir = CalculateLookDirection();
            if (lookDir == Vector3.zero) return;

            var euler = SceneEntity.Rotation.eulerAngles;
            var dir = Quaternion.LookRotation(lookDir, SceneEntity.Up);
            var euler2 = dir.eulerAngles;
            var angle = euler2.y - euler.y;
            while (angle < -180) angle += 360;
            while (angle > 180) angle -= 360;

            if (Mathf.Abs(angle) > CharacterInput.RotateSpeed * 0.01f)
            {
                var deltaAngle = CharacterInput.RotateSpeed * deltaTime * (angle < 0 ? -1 : 1);
                float newY = euler.y + (angle < 0 ? Mathf.Max(deltaAngle, angle) : Mathf.Min(deltaAngle, angle));
                SceneEntity.Rotation = Quaternion.Euler(euler.x, newY, euler.z);
            }
            else
            {
                SceneEntity.Rotation = Quaternion.Euler(euler.x, euler2.y, euler.z);
            }
        }

        /// <summary>
        /// 应用 ORCA 避障，返回处理后的速度
        /// </summary>
        protected Vector3 ApplyORCA(Vector3 velocity, float speed)
        {
            var orcaAgent = parent.GetComponent<ORCAAgentComponent>();
            orcaAgent?.SetVelocity(velocity, speed);
            if (orcaAgent != null)
            {
                velocity = orcaAgent.GetVelocity();
            }
            return velocity;
        }
    }

    public abstract class MoveComponent<T> : MoveComponent, IComponent<T> where T: ConfigMoveAgent
    {
        public T ConfigMove => Config as T;
        public void Init(T config)
        {
            base.Init(config);
            InitInternal();
        }

        public sealed override void Destroy()
        {
            base.Destroy();
            DestroyInternal();
        }

        protected abstract void InitInternal();
        protected abstract void DestroyInternal();
    }
}