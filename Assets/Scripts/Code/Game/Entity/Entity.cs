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
                if(item.Key != item.Value.GetType()) continue;
                item.Value?.BeforeDestroy();
                (item.Value as IComponentDestroy)?.Destroy();
                item.Value?.AfterDestroy();
            }

            Components.Dispose();
            Components = null;
            OtherComponents.Dispose();
            OtherComponents = null;
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
            Components = DictionaryComponent<Type, Component>.Create();
            OtherComponents = DictionaryComponent<Type, Component>.Create();
            CreateTime = GameTimerManager.Instance.GetTimeNow();
        }

        #endregion



        #region 扩展数据

        /// <summary>
        /// 自己的Component
        /// </summary>
        protected DictionaryComponent<Type, Component> Components;
        /// <summary>
        /// 添加共用其他人的Component，不处理生命周期
        /// </summary>
        protected DictionaryComponent<Type, Component> OtherComponents;
        public T AddComponent<T>(Type baseType = null) where T : Component
        {
            if (baseType != null && Components.ContainsKey(baseType))
            {
                Log.Error($"重复添加{baseType.Name}");
                return default;
            }
            Type type = TypeInfo<T>.Type;
            if (Components.ContainsKey(type))
            {
                Log.Error($"重复添加{type.Name}");
                return default;
            }

            T data = ObjectPool.Instance.Fetch(type) as T;
            data.BeforeInit(this);
            Components.Add(type, data);
            if (baseType != null) Components.Add(baseType, data);
            if (data is IComponent comp)
                comp.Init();
            data.AfterInit();
            return data;
        }

        public T AddComponent<T, P1>(P1 p1,Type baseType = null) where T : Component, IComponent<P1>
        {
            if (baseType != null && Components.ContainsKey(baseType))
            {
                Log.Error($"重复添加{baseType.Name}");
                return default;
            }
            Type type = TypeInfo<T>.Type;
            if (Components.ContainsKey(type))
            {
                Log.Error($"重复添加{type.Name}");
                return default;
            }

            T data = ObjectPool.Instance.Fetch(type) as T;
            data.BeforeInit(this);
            Components.Add(type, data);
            if (baseType != null) Components.Add(baseType, data);
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

        /// <summary>
        /// 获取自身或绑定他人的组件
        /// </summary>
        /// <param name="includeOther">包括绑定他人的？</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetComponent<T>(bool includeOther = true) where T : Component, IComponentDestroy
        {
            Type type = TypeInfo<T>.Type;
            if (includeOther && OtherComponents.TryGetValue(type, out var res))
            {
                if (res.IsDispose)
                {
                    OtherComponents.Remove(type);
                }
                else
                {
                    return (T) res;
                }
            }
            if (Components.TryGetValue(type, out res))
            {
                return (T) res;
            }

            // Log.Error($"不存在{type.Name}");
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

        public void RemoveComponent<T>(T t) where T : Component
        {
            Type type = TypeInfo<T>.Type;
            RemoveComponent(type);
        }
        public void RemoveComponent<T>() where T : Component
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

        public void AddOtherComponent<T>(T t) where T : Component
        {
            if (t == null) return;
            OtherComponents[t.GetType()] = t;
        }
        
        public void RemoveOtherComponent<T>(T t) where T : Component
        {
            if (t == null) return;
            if (OtherComponents.TryGetValue(t.GetType(), out var res) && res == t)
            {
                OtherComponents.Remove(t.GetType());
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