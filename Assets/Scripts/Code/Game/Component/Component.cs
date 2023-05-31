using System;

namespace TaoTie
{
    /// <summary>
    /// 所有组件都是从池子中取的，回收时一定要通过Destroy方法将数据清掉
    /// </summary>
    public abstract class Component : IDisposable
    {
        [Timer(TimerType.ComponentUpdate)]
        public class ComponentUpdate : ATimer<IUpdate>
        {
            public override void Run(IUpdate t)
            {
                try
                {
                    t.Update();
                }
                catch (Exception ex)
                {
                    Log.Error(ex);
                }
            }
        }
        protected Entity parent { get; private set; }
        public long Id => parent != null ? parent.Id : 0;
        private long timerId;
        public void BeforeInit(Entity entity)
        {
            IsDispose = false;
            parent = entity;
        }
        public void AfterInit()
        {
            if(this is IUpdate updater)
                timerId = GameTimerManager.Instance.NewFrameTimer(TimerType.ComponentUpdate, updater);
        }
        public void BeforeDestroy()
        {
            IsDispose = true;
            GameTimerManager.Instance?.Remove(ref timerId);
        }
        public void AfterDestroy()
        {
            parent = null;
            ObjectPool.Instance.Recycle(this);
        }

        public bool IsDispose { get; private set; }

        public void Dispose()
        {
            if (IsDispose) return;
            BeforeDestroy();
            (this as IComponentDestroy)?.Destroy();
            if (parent != null)
            {
                parent.RemoveComponent(GetType());
            }
            AfterDestroy();
        }

        public T GetParent<T>() where T : Entity
        {
            return parent as T;
        }

        public T GetComponent<T>() where T : Component, IComponentDestroy
        {
            return parent?.GetComponent<T>();
        }
    }
}