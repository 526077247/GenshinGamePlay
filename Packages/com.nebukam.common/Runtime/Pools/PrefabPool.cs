// Copyright (c) 2021 Timothé Lapetite - nebukam@gmail.com
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace Nebukam
{

    public static class Prefabs
    {

        public delegate void OnPrefabReleased(IPoolablePrefabItem node);
        private static Dictionary<GameObject, Dictionary<Type, IPrefabPool>> m_pools = new Dictionary<GameObject, Dictionary<Type, IPrefabPool>>();

        private static GameObject m_poolGO;
        private static Transform m_poolParent;
        internal static Transform poolWrapper { get { return m_poolParent; } }

        [RuntimeInitializeOnLoadMethod]
        static void RunOnStart()
        {

            m_poolGO = new GameObject("Nebukam.PrefabsPool");
            m_poolGO.SetActive(false);
            m_poolParent = m_poolGO.transform;

            GameObject.DontDestroyOnLoad(m_poolGO);

            //m_poolContainer.hideFlags = HideFlags.HideInHierarchy;

        }

        #region GetPool

        internal static PrefabPool<T> GetPool<T>(GameObject asset)
            where T : PoolablePrefab, new()
        {

            Dictionary<Type, IPrefabPool> poolList;
            PrefabPool<T> pool;

            if (!m_pools.TryGetValue(asset, out poolList))
            {
                poolList = new Dictionary<Type, IPrefabPool>();
                m_pools[asset] = poolList;
            }


            Type type = typeof(T);
            IPrefabPool result;

            if (!poolList.TryGetValue(type, out result))
            {
                pool = new PrefabPool<T>(asset);
                poolList[type] = pool;
            }
            else
            {
                pool = result as PrefabPool<T>;
            }

            return pool;

        }

        internal static IPrefabPool GetPool(Type nodeType, GameObject asset)
        {

#if UNITY_EDITOR
            if (!typeof(PoolablePrefab).IsAssignableFrom(nodeType))
            {
                throw new ArgumentException("Type must derive from PoolablePrefab.", "type");
            }
#endif

            Dictionary<Type, IPrefabPool> poolList;
            IPrefabPool pool;

            if (!m_pools.TryGetValue(asset, out poolList))
            {
                poolList = new Dictionary<Type, IPrefabPool>();
                m_pools[asset] = poolList;
            }

            if (!poolList.TryGetValue(nodeType, out pool))
            {
                pool = Activator.CreateInstance(typeof(PrefabPool<>).MakeGenericType(nodeType)) as IPrefabPool;
                pool.prefab = asset;
                poolList[nodeType] = pool;
            }

            return pool;

        }

        #endregion

        #region Return

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public static bool Return(PoolablePrefab node)
        {

#if UNITY_EDITOR
            if (node.instanceOf == null)
                throw new Exception("Returning PoolablePrefabItem with instanceOf set to null. Make sure you use PrefabsPool.Rent() to create poolable prefab instances.");
#endif

            return GetPool(node.__type, node.instanceOf).Return(node);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="node"></param>
        /// <returns></returns>
        public static bool Return<T>(T node)
            where T : PoolablePrefab, new()
        {

#if UNITY_EDITOR
            if (node.instanceOf == null)
                throw new Exception("Returning PoolablePrefabItem with instanceOf set to null. Make sure you use PrefabsPool.Rent() to create poolable prefab instances.");
#endif

            return GetPool<T>(node.instanceOf).Return(node);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static bool Return(GameObject item)
        {

            PoolablePrefab node = item.GetComponent<PoolablePrefab>();

#if UNITY_EDITOR
            if (node == null)
                throw new Exception("Returning Prefab without PoolablePrefab component. Make sure you use PrefabsPool.Rent() to create poolable prefab instances.");
#endif

            return GetPool(node.__type, node.instanceOf).Return(node);

        }

        #endregion

        #region Rent

        /// <summary>
        /// Create a new instance of a given asset or retrieve a pooled one.
        /// </summary>
        /// <param name="asset">Prefab to be instanced</param>
        /// <param name="parent">Parent transform to add the instance to</param>
        /// <returns></returns>
        public static GameObject Rent(GameObject asset, Transform parent = null)
        {
            PoolablePrefab node = asset.GetComponent<PoolablePrefab>();

            if (node == null)
                return GetPool<PoolablePrefab>(asset).Rent(parent).gameObject;

            return GetPool(node.__type, asset).InternalRent(parent).gameObject;

        }

        /// <summary>
        /// Create a new instance of a given asset or retrieve a pooled one.
        /// </summary>
        /// <typeparam name="T">Type of PoolablePrefab to be instanced</typeparam>
        /// <param name="asset">Prefab to be instanced</param>
        /// <param name="parent">Parent transform to add the instance to</param>
        /// <returns></returns>
        public static T Rent<T>(GameObject asset, Transform parent = null)
            where T : PoolablePrefab, new()
        {
#if UNITY_EDITOR
            PoolablePrefab node = asset.GetComponent<PoolablePrefab>();
            if (node != null && node.GetType() != typeof(T))
                throw new Exception("Asset already has a PoolablePrefab ( " + node.GetType().Name + " ) attached, and isn't of the requested type ( " + typeof(T).Name + " )");
#endif

            return GetPool<T>(asset).Rent(parent) as T;
        }

        #endregion

        #region Preload

        /// <summary>
        /// Pre-instanciate a given number prefabs to avoid overhead associated
        /// to instanciation later on.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="asset">Prefab to create instances of</param>
        /// <param name="count">Number of prefabs to be created</param>
        public static void Preload<T>(GameObject asset, int count)
            where T : PoolablePrefab, new()
        {
            GetPool<T>(asset).Preload(count);
        }

        /// <summary>
        /// Pre-instanciate a given number prefabs to avoid overhead associated
        /// to instanciation later on.
        /// </summary>
        /// <param name="asset">Prefab to create instances of</param>
        /// <param name="count">Number of prefabs to be created</param>
        public static void Preload(GameObject asset, int count)
        {
            PoolablePrefab node = asset.GetComponent<PoolablePrefab>();
            if (node == null)
                GetPool<PoolablePrefab>(asset).Preload(count);
            else
                GetPool(node.__type, asset).InternalPreload(count);
        }

        #endregion

        #region Flush

        /// <summary>
        /// Flushes all pools and force a pass of garbage collection.
        /// Make sure you know what you're doing.
        /// </summary>
        public static void Flush()
        {

            KeyValuePair<GameObject, Dictionary<Type, IPrefabPool>>[] types = m_pools.ToArray();
            KeyValuePair<GameObject, Dictionary<Type, IPrefabPool>> kvp;

            for (int i = 0, count = types.Length; i < count; i++)
            {
                kvp = types[i];

                KeyValuePair<Type, IPrefabPool>[] subtypes = kvp.Value.ToArray();

                for (int j = 0, jcount = subtypes.Length; j < jcount; j++)
                {
                    subtypes[j].Value.Flush();
                }

            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="asset"></param>
        public static void Flush(GameObject asset)
        {

            Dictionary<Type, IPrefabPool> poolList;

            if (!m_pools.TryGetValue(asset, out poolList))
                return;

            IPrefabPool pool;
            PoolablePrefab node = asset.GetComponent<PoolablePrefab>();
            Type type = node == null ? typeof(PoolablePrefab) : node.__type;

            if (!poolList.TryGetValue(type, out pool))
                return;

            pool.Flush();

        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="asset"></param>
        public static void Flush<T>(GameObject asset)
        {

            Dictionary<Type, IPrefabPool> poolList;

            if (!m_pools.TryGetValue(asset, out poolList))
                return;

            IPrefabPool pool;
            if (!poolList.TryGetValue(typeof(T), out pool))
                return;

            pool.Flush();

        }

        #endregion

#if UNITY_EDITOR

        /// <summary>
        /// EDITOR-ONLY : List statistics of the currently available pools
        /// </summary>
        public static void PrintStats()
        {
            string stats = "";
            KeyValuePair<GameObject, Dictionary<Type, IPrefabPool>>[] types = m_pools.ToArray();
            KeyValuePair<GameObject, Dictionary<Type, IPrefabPool>> kvp;

            for (int i = 0, count = types.Length; i < count; i++)
            {

                kvp = types[i];

                KeyValuePair<Type, IPrefabPool>[] subtypes = kvp.Value.ToArray();
                KeyValuePair<Type, IPrefabPool> subkvp;

                for (int j = 0, jcount = subtypes.Length; j < jcount; j++)
                {
                    subkvp = subtypes[j];
                    stats += String.Format("Pool<{0} · {1}> : {2} available, {3} created ({4} destroyed).\n",
                        kvp.Key.name,
                        subkvp.Key.Name,
                        subkvp.Value.poolSize,
                        subkvp.Value.newTicker,
                        subkvp.Value.destroyTicker);
                }

            }
            UnityEngine.Debug.Log(stats);
        }

#endif

        /// <summary>
        /// 
        /// </summary>
        /// <param name="this"></param>
        /// <param name="returnDelegate"></param>
        public static void onRelease(this PoolablePrefab @this, Prefabs.OnPrefabReleased returnDelegate)
        {

            if ((@this as IPoolablePrefabNode).__released) { return; }

            List<Prefabs.OnPrefabReleased> list = @this.__onRelease;

            if (list == null)
            {
                list = new List<Prefabs.OnPrefabReleased>();
                @this.__onRelease = list;
            }

            if (!list.Contains(returnDelegate))
                list.Add(returnDelegate);

            return;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="this"></param>
        /// <param name="returnDelegate"></param>
        public static void offRelease(this PoolablePrefab @this, Prefabs.OnPrefabReleased returnDelegate)
        {

            if ((@this as IPoolablePrefabNode).__released) { return; }

            List<Prefabs.OnPrefabReleased> list = @this.__onRelease;

            if (list == null)
                return;

            if (list.TryRemove(returnDelegate))
            {

            }

            return;

        }

    }

    internal interface IPrefabPool
    {
        GameObject prefab { get; set; }
        int poolSize { get; }
        int newTicker { get; }
        int destroyTicker { get; }
        bool Return(IPoolablePrefabNode node);
        PoolablePrefab InternalRent(Transform parent = null);
        void InternalPreload(int count);
        void Flush();
    }

    /// <summary>
    /// A prefab pool is a pool for combinations for a Prefab & its associated IPoolablePrefabNode MonoBehavior.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class PrefabPool<T> : IPrefabPool
        where T : PoolablePrefab, IPoolablePrefabNode, new()
    {

        protected internal GameObject m_prefab = null;

        protected internal int m_poolSize = 0;
        protected internal int m_newTicker = 0;
        protected internal int m_destroyTicker = 0;

        protected internal T m_tail = null;

        int IPrefabPool.poolSize { get { return m_poolSize; } }
        int IPrefabPool.newTicker { get { return m_newTicker; } }
        int IPrefabPool.destroyTicker { get { return m_destroyTicker; } }
        GameObject IPrefabPool.prefab { get => m_prefab; set => m_prefab = value; }

        internal PrefabPool(GameObject asset)
        {
            m_prefab = asset;
        }

        public PrefabPool()
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="count"></param>
        internal void Preload(int count)
        {

            Transform tr = Prefabs.poolWrapper;
            T preloaded;

            if (m_tail == null)
            {
                Instanciate(out preloaded, tr);
                preloaded.gameObject.SetActive(false);
                m_tail = preloaded;
                m_poolSize++;
            }

            while (m_poolSize != count)
            {
                Instanciate(out preloaded, tr);
                preloaded.gameObject.SetActive(false);
                preloaded.__prevNode = m_tail;
                m_tail = preloaded;
                m_poolSize++;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="count"></param>
        void IPrefabPool.InternalPreload(int count) { Preload(count); }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        internal GameObject Instanciate(out T node, Transform parent = null)
        {
            GameObject instance;

            if (parent == null)
            {
                instance = GameObject.Instantiate(m_prefab, Prefabs.poolWrapper);
                instance.SetActive(false);
            }
            else
            {
                instance = GameObject.Instantiate(m_prefab, parent);
            }

            node = instance.GetComponent<T>();

            if (node == null)
                node = instance.AddComponent<T>();

            node.__instanceOf = m_prefab;

            m_newTicker++;

            return instance;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parent"></param>
        /// <returns></returns>
        internal T Rent(Transform parent = null)
        {

            T node;
            GameObject instance;
            IRequireInit init;

            if (m_tail != null)
            {
                node = m_tail;
                m_tail = m_tail.__prevNode as T;
                node.__prevNode = null;
                node.__released = false;
                m_poolSize--;

                node.transform.parent = parent;
                node.gameObject.SetActive(true);

                init = node as IRequireInit;
                if (init != null) { init.Init(); }

                return node;
            }

            instance = Instanciate(out node, parent);
            init = node as IRequireInit;
            if (init != null) { init.Init(); }

            return node;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parent"></param>
        /// <returns></returns>
        PoolablePrefab IPrefabPool.InternalRent(Transform parent = null) { return Rent(parent); }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        bool IPrefabPool.Return(IPoolablePrefabNode node)
        {
            T nAsT = node as T;
            if (nAsT == null) { return false; }
            return Return(nAsT);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        internal bool Return(T node)
        {
            if (node.__released) { return false; }
            node.__released = true;
            //node.gameObject.SetActive(false);
            node.transform.parent = Prefabs.poolWrapper;

            List<Prefabs.OnPrefabReleased> list = node.__onRelease;
            if (list != null)
            {
                for (int i = 0, count = list.Count; i < count; i++)
                    list[i](node);

                list.Clear();
            }

            if (m_tail != null)
                node.__prevNode = m_tail;

            m_tail = node;

            IRequireCleanUp clean = m_tail as IRequireCleanUp;
            if (clean != null) { clean.CleanUp(); }

            m_poolSize++;
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        void IPrefabPool.Flush()
        {
            T node;
            while (m_tail != null)
            {
                node = m_tail;
                m_tail = node.__prevNode as T;
                GameObject.Destroy(node.gameObject);
                m_poolSize--;
                m_destroyTicker++;
            }
        }

        public static implicit operator GameObject(PrefabPool<T> pool)
        {
            return (pool as IPrefabPool).prefab;
        }

    }
}
