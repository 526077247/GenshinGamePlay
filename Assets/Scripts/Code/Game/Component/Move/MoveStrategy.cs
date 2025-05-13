using System;
using Sirenix.Serialization;

namespace TaoTie
{
    [Creatable]
    public abstract class MoveStrategy: IDisposable
    {
        /// <summary>
        /// todo: 此参数为临时处理。为true时，Update之后不进行MoveComponent的Update
        /// </summary>
        public abstract bool OverrideUpdate { get; }
        protected ConfigMoveStrategy configMoveStrategy;
        protected SceneEntity target;
        protected SceneEntity parent;

        protected MoveComponent moveComponent;
        protected bool useAnimMove => moveComponent.useAnimMove;

        public virtual void Init(MoveComponent moveC,ConfigMoveStrategy config)
        {
            moveComponent = moveC;
            parent = moveC.GetParent<SceneEntity>();
            configMoveStrategy = config;
        }
        
        public virtual void Init<P1>(MoveComponent moveC,ConfigMoveStrategy config,P1 p1)
        {
            moveComponent = moveC;
            parent = moveC.GetParent<SceneEntity>();
            configMoveStrategy = config;
            if (this is IMoveStrategy<P1> moveStrategy)
            {
                moveStrategy.SetPara(p1);
            }
        }
        
        public virtual void Dispose()
        {
            moveComponent = null;
            parent = null;
            target = null;
            configMoveStrategy = null;
        }
        public abstract void Update();
        
        public void SetTarget(SceneEntity sceneEntity)
        {
            this.target = sceneEntity;
        }
    }
    
    public abstract class MoveStrategy<T>: MoveStrategy where T : ConfigMoveStrategy
    {
        protected T config => configMoveStrategy as T;
        public sealed override void Init(MoveComponent moveC, ConfigMoveStrategy config)
        {
            base.Init(moveC, config);
            InitInternal();
        }
        public sealed override void Init<P1>(MoveComponent moveC, ConfigMoveStrategy config,P1 p1)
        {
            base.Init(moveC, config, p1);
            InitInternal();
        }

        protected virtual void InitInternal()
        {
            
        }
        
        public sealed override void Dispose()
        {
            DisposeInternal();
            base.Dispose();
        }
        
        protected virtual void DisposeInternal()
        {
            
        }

        public sealed override void Update()
        {
            UpdateInternal();
        }
        
        protected  virtual void UpdateInternal()
        {
            
        }

    }

    public interface IMoveStrategy<P1>
    {
        public void SetPara(P1 p1);
    }
}