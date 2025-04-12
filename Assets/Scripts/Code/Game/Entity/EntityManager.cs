using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TaoTie
{
    public class EntityManager : IManager
    {
        private GameObject root;
        private ListComponent<Entity> entities;
        private DictionaryComponent<long, Entity> idEntityMap;
        private DictionaryComponent<Type, IList> typeEntities;

        public Transform GameObjectRoot => root.transform;

        #region override

        public void Init()
        {
            entities = ListComponent<Entity>.Create();
            idEntityMap = DictionaryComponent<long, Entity>.Create();
            typeEntities = DictionaryComponent<Type, IList>.Create();
            root = new GameObject("EntityRoot");
            GameObject.DontDestroyOnLoad(root);
        }

        public void Destroy()
        {
            for (int i = entities.Count - 1; i >= 0 && entities.Count>0; i--)
            {
                if (i > entities.Count - 1) i = entities.Count - 1;
                entities[i].Dispose();
            }

            entities.Dispose();
            idEntityMap.Dispose();
            typeEntities.Dispose();
            entities = null;
            idEntityMap = null;
            typeEntities = null;
            GameObject.Destroy(root);
        }

        #endregion

        public Entity Get(long id)
        {
            if (idEntityMap.TryGetValue(id, out var res) && !res.IsDispose)
            {
                return res;
            }

            return null;
        }

        public T Get<T>(long id) where T : Entity
        {
            if (idEntityMap.TryGetValue(id, out var res) && !res.IsDispose)
            {
                return res as T;
            }

            return null;
        }

        public bool TryGet(long id, out Entity res)
        {
            if (idEntityMap.TryGetValue(id, out res) && !res.IsDispose)
            {
                return true;
            }

            return false;
        }

        public List<T> GetAll<T>() where T : Entity
        {
            var type = TypeInfo<T>.Type;
            if (typeEntities.TryGetValue(type, out var res))
            {
                return res as List<T>;
            }

            res = new List<T>();
            typeEntities.Add(type, res);
            return res as List<T>;
        }
        public Dictionary<long, Entity> GetAllDict()
        {
            return idEntityMap;
        }
        private void Add<T>(T entity) where T : Entity
        {
            idEntityMap.Add(entity.Id, entity);
            entities.Add(entity);
            if (!typeEntities.ContainsKey(entity.GetType()))
            {
                typeEntities.Add(entity.GetType(), new List<T>());
            }

            typeEntities[entity.GetType()]?.Add(entity);
        }

        public void Remove<T>(T entity) where T : Entity
        {
            idEntityMap.Remove(entity.Id);
            entities.Remove(entity);
            typeEntities[entity.GetType()]?.Remove(entity);
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
            return entities.Count;
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