using UnityEngine;

namespace TaoTie
{
    public abstract partial class MoveComponent: Component,IComponentDestroy,IUpdate
    {
        protected SceneEntity SceneEntity => parent as SceneEntity;
        
        public ConfigMoveStrategy Config { get; private set; }
        public MoveInput CharacterInput{ get; private set; }
        
        protected void Init(ConfigMoveStrategy configMove)
        {
            Config = configMove;
            CharacterInput = new MoveInput();
        }

        public virtual void Destroy()
        {
            FollowMoveDestroy();
            PlatformMoveDestroy();
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
            if(FollowMoveUpdate()) return;
            if(PlatformMoveUpdate()) return;
            UpdateInternal();
        }
        
        protected abstract void UpdateInternal();
    }

    public abstract class MoveComponent<T> : MoveComponent, IComponent<T> where T: ConfigMoveStrategy
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