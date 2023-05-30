using System;


namespace TaoTie
{
    public abstract class Entity : IDisposable
    {
        [Timer(TimerType.DelayDestroyEntity)]
        public class DelayDestroyEntityTimer : ATimer<Entity>
        {
            public override void Run(Entity self)
            {
                try
                {
                    self.Dispose();
                }
                catch (Exception e)
                {
                    Log.Error($"move timer error: {self.Id}\n{e}");
                }
            }
        }
        
        public long Id { get; private set; }
        public bool IsDispose { get; private set; }
        public EntityManager Parent { get; private set; }
        public abstract EntityType Type { get; }
        public long CreateTime { get; private set; }

        private long delayDestroyTimerId;
        #region override

        public void Dispose()
        {
            if (IsDispose) return;
            IsDispose = true;
            if (delayDestroyTimerId != 0) GameTimerManager.Instance.Remove(ref delayDestroyTimerId);
            foreach (var item in Components)
            {
                (item.Value as IComponentDestroy)?.Destroy();
                (item.Value as Component)?.AfterDestroy();
            }

            Components.Dispose();
            Components = null;
            (this as IEntityDestroy)?.Destroy();
            Parent?.Remove(this);
            Parent = null;
            CreateTime = 0;
            ObjectPool.Instance.Recycle(this);
        }

        public void BeforeInit(EntityManager um)
        {
            Parent = um;
            Id = IdGenerater.Instance.GenerateInstanceId();
            IsDispose = false;
            Components = DictionaryComponent<Type, object>.Create();
            CreateTime = GameTimerManager.Instance.GetTimeNow();
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
            Components.Add(type, data);
            if (data is IComponent comp)
                comp.Init();
            data.AfterInit();
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
            Components.Add(type, data);
            data.Init(p1);
            data.AfterInit();
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
            Components.Add(type, data);
            data.Init(p1, p2);
            data.AfterInit();
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
            Components.Add(type, data);
            data.Init(p1, p2, p3);
            data.AfterInit();
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
        
        public T GetOrAddComponent<T>() where T : Component, IComponent
        {
            Type type = TypeInfo<T>.Type;
            if (Components.TryGetValue(type, out var res))
            {
                return (T) res;
            }

            return AddComponent<T>();
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

        public void DelayDispose(long delay)
        {
            if (delayDestroyTimerId != 0) GameTimerManager.Instance.Remove(ref delayDestroyTimerId);
            delayDestroyTimerId = GameTimerManager.Instance.NewOnceTimer(GameTimerManager.Instance.GetTimeNow() + delay,
                TimerType.DelayDestroyEntity, this);
        }
    }
}