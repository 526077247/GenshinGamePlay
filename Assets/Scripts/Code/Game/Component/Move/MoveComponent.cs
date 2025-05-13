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
        /// 强制朝向
        /// </summary>
        /// <param name="target"></param>
        public virtual void ForceLookAt(Vector3 target)
        {
            Vector3 dir = target - SceneEntity.Position;
            dir.y = 0;
            SceneEntity.Rotation = Quaternion.LookRotation(dir, Vector3.up);
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