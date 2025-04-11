using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TaoTie
{
    public class EntityManager : IManager
    {
        private GameObject Root;
        private ListComponent<Entity> Entitys;
        private DictionaryComponent<long, Entity> IdEntityMap;
        private DictionaryComponent<Type, IList> TypeEntitys;

        public Transform GameObjectRoot => Root.transform;

        #region override

        public void Init()
        {
            Entitys = ListComponent<Entity>.Create();
            IdEntityMap = DictionaryComponent<long, Entity>.Create();
            TypeEntitys = DictionaryComponent<Type, IList>.Create();
            Root = new GameObject("EntityRoot");
            GameObject.DontDestroyOnLoad(Root);
        }

        public void Destroy()
        {
            for (int i = Entitys.Count - 1; i >= 0 && Entitys.Count>0; i--)
            {
                if (i > Entitys.Count - 1) i = Entitys.Count - 1;
                Entitys[i].Dispose();
            }

            Entitys.Dispose();
            IdEntityMap.Dispose();
            TypeEntitys.Dispose();
            Entitys = null;
            IdEntityMap = null;
            TypeEntitys = null;
            GameObject.Destroy(Root);
        }

        #endregion

        public Entity Get(long id)
        {
            if (IdEntityMap.TryGetValue(id, out var res) && !res.IsDispose)
            {
                return res;
            }

            return null;
        }

        public T Get<T>(long id) where T : Entity
        {
            if (IdEntityMap.TryGetValue(id, out var res) && !res.IsDispose)
            {
                return res as T;
            }

            return null;
        }

        public bool TryGet(long id, out Entity res)
        {
            if (IdEntityMap.TryGetValue(id, out res) && !res.IsDispose)
            {
                return true;
            }

            return false;
        }

        public List<T> GetAll<T>() where T : Entity
        {
            var type = TypeInfo<T>.Type;
            if (TypeEntitys.TryGetValue(type, out var res))
            {
                return res as List<T>;
            }

            res = new List<T>();
            TypeEntitys.Add(type, res);
            return res as List<T>;
        }

        private void Add<T>(T entity) where T : Entity
        {
            IdEntityMap.Add(entity.Id, entity);
            Entitys.Add(entity);
            if (!TypeEntitys.ContainsKey(entity.GetType()))
            {
                TypeEntitys.Add(entity.GetType(), new List<T>());
            }

            TypeEntitys[entity.GetType()]?.Add(entity);
        }

        public void Remove<T>(T entity) where T : Entity
        {
            IdEntityMap.Remove(entity.Id);
            Entitys.Remove(entity);
            TypeEntitys[entity.GetType()]?.Remove(entity);
            entity.Dispose();
        }

        public void Remove(long id)
        {
            if (TryGet(id, out var entity))
            {
                Remove(entity);
            }
        }

        public int GetTotal()
        {
            return Entitys.Count;
        }

        public T CreateEntity<T>() where T : Entity, IEntity
        {
            T res = ObjectPool.Instance.Fetch<T>();
            res.BeforeInit(this);
            res.Init();
            Add(res);
            return res;
        }

        public T CreateEntity<T, P1>(P1 p1) where T : Entity, IEntity<P1>
        {
            T res = ObjectPool.Instance.Fetch<T>();
            res.BeforeInit(this);
            res.Init(p1);
            Add(res);
            return res;
        }

        public T CreateEntity<T, P1, P2>(P1 p1, P2 p2) where T : Entity, IEntity<P1, P2>
        {
            T res = ObjectPool.Instance.Fetch<T>();
            res.BeforeInit(this);
            res.Init(p1, p2);
            Add(res);
            return res;
        }

        public T CreateEntity<T, P1, P2, P3>(P1 p1, P2 p2, P3 p3) where T : Entity, IEntity<P1, P2, P3>
        {
            T res = ObjectPool.Instance.Fetch<T>();
            res.BeforeInit(this);
            res.Init(p1, p2, p3);
            Add(res);
            return res;
        }

        public T CreateEntity<T, P1, P2, P3, P4>(P1 p1, P2 p2, P3 p3, P4 p4) where T : Entity, IEntity<P1, P2, P3, P4>
        {
            T res = ObjectPool.Instance.Fetch<T>();
            res.BeforeInit(this);
            res.Init(p1, p2, p3, p4);
            Add(res);
            return res;
        }

        public T CreateEntity<T, P1, P2, P3, P4, P5>(P1 p1, P2 p2, P3 p3, P4 p4, P5 p5)
            where T : Entity, IEntity<P1, P2, P3, P4, P5>
        {
            T res = ObjectPool.Instance.Fetch<T>();
            res.BeforeInit(this);
            res.Init(p1, p2, p3, p4, p5);
            Add(res);
            return res;
        }
    }
}