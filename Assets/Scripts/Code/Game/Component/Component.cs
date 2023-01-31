using System;

namespace TaoTie
{
    /// <summary>
    /// 所有组件都是从池子中取的，回收时一定要通过Destroy方法将数据清掉
    /// </summary>
    public abstract class Component : IDisposable
    {
        protected Entity Parent;
        public long Id => Parent != null ? Parent.Id : 0;

        public void BeforeInit(Entity entity)
        {
            Parent = entity;
        }

        public void AfterDestroy()
        {
            Parent = null;
        }

        public bool IsDispose { get; private set; }

        public void Dispose()
        {
            if (IsDispose) return;
            IsDispose = true;
            (this as IComponentDestroy)?.Destroy();
            if (Parent != null)
            {
                Parent.RemoveComponent(GetType());
                Parent = null;
            }

            ObjectPool.Instance.Recycle(this);
        }

        public T GetParent<T>() where T : Entity
        {
            return Parent as T;
        }
    }
}