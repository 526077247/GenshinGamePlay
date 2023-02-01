using System;


namespace TaoTie
{
    public abstract class Entity : IDisposable
    {
        public long Id { get; private set; }
        public bool IsDispose { get; private set; }
        public EntityManager Parent { get; private set; }

        #region override

        public void Dispose()
        {
            if (IsDispose) return;
            IsDispose = true;
            (this as IEntityDestroy)?.Destroy();
            Parent?.Remove(this);
            Parent = null;
            foreach (var item in Components)
            {
                (item.Value as IComponentDestroy)?.Destroy();
                (item.Value as Component)?.AfterDestroy();
            }

            Components.Dispose();
            Components = null;
            ObjectPool.Instance.Recycle(this);
        }

        public void BeforeInit(EntityManager um)
        {
            Parent = um;
            Id = IdGenerater.Instance.GenerateInstanceId();
            IsDispose = false;
            Components = DictionaryComponent<Type, object>.Create();
        }

        #endregion



        #region 扩展数据

        protected DictionaryComponent<Type, object> Components;

        public T AddComponent<T>() where T : Component
        {
            Type type = TypeInfo<T>.Type;
            if (Components.ContainsKey(type))
            {
                Log.Error($"重复添加{type.Name}");
                return default;
            }

            T data = ObjectPool.Instance.Fetch(type) as T;
            data.BeforeInit(this);
            if (data is IComponent comp)
                comp.Init();
            Components.Add(type, data);
            return data;
        }

        public T AddComponent<T, P1>(P1 p1) where T : Component, IComponent<P1>
        {
            Type type = TypeInfo<T>.Type;
            if (Components.ContainsKey(type))
            {
                Log.Error($"重复添加{type.Name}");
                return default;
            }

            T data = ObjectPool.Instance.Fetch(type) as T;
            data.BeforeInit(this);
            data.Init(p1);
            Components.Add(type, data);
            return data;
        }

        public T AddComponent<T, P1, P2>(P1 p1, P2 p2) where T : Component, IComponent<P1, P2>
        {
            Type type = TypeInfo<T>.Type;
            if (Components.ContainsKey(type))
            {
                Log.Error($"重复添加{type.Name}");
                return default;
            }

            T data = ObjectPool.Instance.Fetch(type) as T;
            data.BeforeInit(this);
            data.Init(p1, p2);
            Components.Add(type, data);
            return data;
        }

        public T AddComponent<T, P1, P2, P3>(P1 p1, P2 p2, P3 p3) where T : Component, IComponent<P1, P2, P3>
        {
            Type type = TypeInfo<T>.Type;
            if (Components.ContainsKey(type))
            {
                Log.Error($"重复添加{type.Name}");
                return default;
            }

            T data = ObjectPool.Instance.Fetch(type) as T;
            data.BeforeInit(this);
            data.Init(p1, p2, p3);
            Components.Add(type, data);
            return data;
        }

        public T GetComponent<T>() where T : Component, IComponentDestroy
        {
            Type type = TypeInfo<T>.Type;
            if (Components.TryGetValue(type, out var res))
            {
                return (T) res;
            }

            Log.Error($"不存在{type.Name}");
            return default;
        }

        public void RemoveComponent<T>() where T : Component, IComponentDestroy
        {
            Type type = TypeInfo<T>.Type;
            RemoveComponent(type);
        }

        public void RemoveComponent(Type type)
        {
            if (Components.TryGetValue(type, out var res))
            {
                Components.Remove(type);
                (res as Component)?.Dispose();
            }
        }

        #endregion
    }
}