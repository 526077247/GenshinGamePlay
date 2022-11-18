using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace TaoTie
{
    /// <summary>
    /// <para>GameObject缓存池</para>
    /// <para>注意：</para>
    /// <para>1、所有需要预设都从这里加载，不要直接到ResourcesManager去加载，由这里统一做缓存管理</para>
    /// <para>2、缓存分为两部分：从资源层加载的原始GameObject(Asset)，从GameObject实例化出来的多个Inst</para>
    /// <para>原则: 有借有还，再借不难，借出去的东西回收时，请不要污染(pc上会进行检测，发现则报错)</para>
    /// <para>何为污染？不要往里面添加什么节点，借出来的是啥样子，返回来的就是啥样子</para>
    /// <para>GameObject内存管理，采用lru cache来管理prefab</para>
    /// <para>为了对prefab和其产生的go的内存进行管理，所以严格管理go生命周期</para>
    /// <para>1、创建 -> GetGameObjectAsync</para>
    /// <para>2、回收 -> 绝大部分的时候应该采用回收(回收go不能被污染)，对象的销毁对象池会自动管理 RecycleGameObject</para>
    /// <para>3、销毁 -> 如果的确需要销毁，或一些用不到的数据想要销毁，也必须从这GameObjectPool中进行销毁，
    ///         严禁直接调用GameObject.Destroy方法来进行销毁，而应该采用GameObjectPool.DestroyGameObject方法</para>
    /// <para>不管是销毁还是回收，都不要污染go，保证干净</para>
    /// </summary>
    public class GameObjectPoolManager:IManager
    {

        public static GameObjectPoolManager Instance { get; private set; }
        private Transform __cacheTransRoot;

        private LruCache<string, GameObject> __goPool;
        private Dictionary<string, int> __goInstCountCache;//go: inst_count 用于记录go产生了多少个实例

        private Dictionary<string, int> __goChildsCountPool;//path: child_count 用于在editor模式下检测回收的go是否被污染 path:num

        private Dictionary<string, List<GameObject>> __instCache; //path: inst_array
        private Dictionary<GameObject, string> __instPathCache;// inst : prefab_path 用于销毁和回收时反向找到inst对应的prefab TODO:这里有优化空间path太占内存
        private Dictionary<string, bool> __persistentPathCache;//需要持久化的资源
        private Dictionary<string, Dictionary<string, int>> __detailGoChildsCount;//记录go子控件具体数量信息
        
        #region override
        
        public void Init()
        {
	        Instance = this;
            this.__goPool = new LruCache<string, GameObject>();
            this.__goInstCountCache = new Dictionary<string, int>();
            this.__goChildsCountPool = new Dictionary<string, int>();
            this.__instCache = new Dictionary<string, List<GameObject>>();
            this.__instPathCache = new Dictionary<GameObject, string>();
            this.__persistentPathCache = new Dictionary<string, bool>();
            this.__detailGoChildsCount = new Dictionary<string, Dictionary<string, int>>();

            var go = GameObject.Find("GameObjectCacheRoot");
            if (go == null)
            {
                go = new GameObject("GameObjectCacheRoot");
            }
            GameObject.DontDestroyOnLoad(go);
            this.__cacheTransRoot = go.transform;

            this.__goPool.SetPopCallback((path, pooledGo) =>
            {
                this.__ReleaseAsset(path);
            });
            this.__goPool.SetCheckCanPopCallback((path, pooledGo) =>
            {
                var cnt = this.__goInstCountCache[path] - (this.__instCache.ContainsKey(path) ? this.__instCache[path].Count : 0);
                if (cnt > 0)
                    Log.Info(string.Format("path={0} __goInstCountCache={1} __instCache={2}", path, this.__goInstCountCache[path], 
                        (this.__instCache[path] != null ? this.__instCache[path].Count : 0)));
                return cnt == 0 && !this.__persistentPathCache.ContainsKey(path);
            });
        }

        public void Destroy()
        {
	        Instance = null;
            this.Cleanup();
        }
        
        #endregion


        /// <summary>
		/// 预加载一系列资源
		/// </summary>
		/// <param name="this"></param>
		/// <param name="res"></param>
		public async ETTask LoadDependency(List<string> res)
		{
			if (res.Count <= 0) return;
			using (ListComponent<ETTask> TaskScheduler = ListComponent<ETTask>.Create())
			{
				for (int i = 0; i < res.Count; i++)
				{
					TaskScheduler.Add(this.PreLoadGameObjectAsync(res[i], 1));
				}
				await ETTaskHelper.WaitAll(TaskScheduler);
			}
		}
		/// <summary>
		/// 尝试从缓存中获取
		/// </summary>
		/// <param name="path"></param>
		/// <param name="go"></param>
		/// <returns></returns>
		public bool TryGetFromCache(string path, out GameObject go)
		{
			go = null;
			if (!this.CheckHasCached(path)) return false;
			if (this.__instCache.TryGetValue(path, out var cachedInst))
			{
				if (cachedInst.Count > 0)
				{
					var inst = cachedInst[cachedInst.Count - 1];
					cachedInst.RemoveAt(cachedInst.Count - 1);
					go = inst;
					if (inst == null)
					{
						Log.Error("Something wrong, there gameObject instance in cache is null!");
						return false;
					}
					return true;
				}
			}
			if (this.__goPool.TryGet(path, out var pooledGo))
			{
				if (pooledGo != null)
				{
					var inst = GameObject.Instantiate(pooledGo);
					this.__goInstCountCache[path] = this.__goInstCountCache[path] + 1;
					this.__instPathCache[inst] = path;
					go = inst;
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// 预加载：可提供初始实例化个数
		/// </summary>
		/// <param name="path"></param>
		/// <param name="inst_count">初始实例化个数</param>
		/// <param name="callback"></param>
		public async ETTask PreLoadGameObjectAsync(string path, int inst_count,Action callback = null)
		{
			CoroutineLock coroutineLock = null;
			try
			{
				coroutineLock = await CoroutineLockManager.Instance.Wait(CoroutineLockType.Resources, path.GetHashCode());
				if (this.CheckHasCached(path))
				{
					callback?.Invoke();
				}
				else
				{
					var go = await ResourcesManager.Instance.LoadAsync<GameObject>(path);
					if (go != null)
					{
						this.CacheAndInstGameObject(path, go as GameObject, inst_count);
					}
					callback?.Invoke();
				}
			}
			finally
			{
				coroutineLock?.Dispose();
			}
		}
		/// <summary>
		/// 异步获取：必要时加载
		/// </summary>
		/// <param name="path"></param>
		/// <param name="callback"></param>
		/// <returns></returns>
		public ETTask GetGameObjectTask( string path, Action<GameObject> callback = null)
		{
			ETTask task = ETTask.Create();
			this.GetGameObjectAsync(path, (data) =>
			{
				callback?.Invoke(data);
				task.SetResult();
			}).Coroutine();
			return task;
		}

		/// <summary>
		/// 异步获取：必要时加载
		/// </summary>
		/// <param name="path"></param>
		/// <param name="callback"></param>
		/// <returns></returns>
		public async ETTask<GameObject> GetGameObjectAsync(string path,Action<GameObject> callback = null)
		{
			if (this.TryGetFromCache(path, out var inst))
			{
				this.InitInst(inst);
				callback?.Invoke(inst);
				return inst;
			}
			await this.PreLoadGameObjectAsync(path, 1);
			if (this.TryGetFromCache(path, out inst))
			{
				this.InitInst(inst);
				callback?.Invoke(inst);
				return inst;
			}
			callback?.Invoke(null);
			return null;
		}

		/// <summary>
		/// 同步取已加载的，没加载过则返回null
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public GameObject GetGameObject(string path)
		{
			if (this.TryGetFromCache(path, out var inst))
			{
				this.InitInst(inst);
				return inst;
			}
			return null;
		}
		/// <summary>
		/// 回收
		/// </summary>
		/// <param name="inst"></param>
		/// <param name="isclear"></param>
		public void RecycleGameObject(GameObject inst, bool isclear = false)
		{
			if (!this.__instPathCache.ContainsKey(inst))
			{
				Log.Error("RecycleGameObject inst not found from __instPathCache");
				return;
			}
			var path = this.__instPathCache[inst];
			if (!isclear)
			{
				this.__CheckRecycleInstIsDirty(path, inst, null);
				inst.transform.SetParent(this.__cacheTransRoot, false);
				inst.SetActive(false);
				if (!this.__instCache.ContainsKey(path))
				{
					this.__instCache[path] = new List<GameObject>();
				}
				this.__instCache[path].Add(inst);
			}
			else
			{
				this.DestroyGameObject(inst);
			}

			//this.CheckCleanRes(path);
		}

		/// <summary>
		/// <para>检测回收的时候是否需要清理资源(这里是检测是否清空 inst和缓存的go)</para>
		/// <para>这里可以考虑加一个配置表来处理优先级问题，一些优先级较高的保留</para>
		/// </summary>
		/// <param name="path"></param>
		public void CheckCleanRes(string path)
		{
			var cnt = this.__goInstCountCache[path] - (this.__instCache.ContainsKey(path) ? this.__instCache[path].Count : 0);
			if (cnt == 0 && !this.__persistentPathCache.ContainsKey(path))
				this.__ReleaseAsset(path);
		}

		/// <summary>
		/// <para>添加需要持久化的资源</para>
		/// </summary>
		/// <param name="path"></param>
		public void AddPersistentPrefabPath(string path)
		{
			this.__persistentPathCache[path] = true;

		}
		
		/// <summary>
		/// <para>清理缓存</para>
		/// </summary>
		/// <param name="includePooledGo">是否需要将预设也释放</param>
		/// <param name="excludePathArray">忽略的</param>
		public void Cleanup(bool includePooledGo = true, List<string> excludePathArray = null)
		{
			Log.Info("GameObjectPool Cleanup ");
			foreach (var item in this.__instCache)
			{
				for (int i = 0; i < item.Value.Count; i++)
				{
					var inst = item.Value[i];
					if (inst != null)
					{
						GameObject.Destroy(inst);
						this.__goInstCountCache[item.Key]--;
					}
					this.__instPathCache.Remove(inst);
				}
			}
			this.__instCache = new Dictionary<string, List<GameObject>>();

			if (includePooledGo)
			{
				Dictionary<string, bool> dict_excludepath = null;
				if (excludePathArray != null)
				{
					dict_excludepath = new Dictionary<string, bool>();
					for (int i = 0; i < excludePathArray.Count; i++)
					{
						dict_excludepath[excludePathArray[i]] = true;
					}
				}

				List<string> keys = this.__goPool.Keys.ToList();
				for (int i = keys.Count - 1; i >= 0; i--)
				{
					var path = keys[i];
					if (dict_excludepath != null && !dict_excludepath.ContainsKey(path) && this.__goPool.TryOnlyGet(path, out var pooledGo))
					{
						if (pooledGo != null && this.__CheckNeedUnload(path))
						{
							ResourcesManager.Instance.ReleaseAsset(pooledGo);
							this.__goPool.Remove(path);
						}
					}
				}
			}
			Log.Info("GameObjectPool Cleanup Over");
		}

		/// <summary>
		/// <para>释放asset</para>
		/// <para>注意这里需要保证外面没有引用这些path的inst了，不然会出现材质丢失的问题</para>
		/// <para>不要轻易调用，除非你对内部的资源的生命周期有了清晰的了解</para>
		/// </summary>
		/// <param name="includePooledGo">是否需要将预设也释放</param>
		/// <param name="patharray">需要释放的资源路径数组</param>
		public void CleanupWithPathArray(bool includePooledGo = true, List<string> patharray = null)
		{
			Debug.Log("GameObjectPool Cleanup ");
			Dictionary<string, bool> dict_path = null;
			if (patharray != null)
			{
				dict_path = new Dictionary<string, bool>();
				for (int i = 0; i < patharray.Count; i++)
				{
					dict_path[patharray[i]] = true;
				}
			}
			foreach (var item in this.__instCache)
			{
				if (dict_path.ContainsKey(item.Key))
				{
					for (int i = 0; i < item.Value.Count; i++)
					{
						var inst = item.Value[i];
						if (inst != null)
						{
							GameObject.Destroy(inst);
							this.__goInstCountCache[item.Key]-- ;
						}
						this.__instPathCache.Remove(inst);
					}
				}
			}
			for (int i = 0; i < patharray.Count; i++)
			{
				this.__instCache.Remove(patharray[i]);
			}

			if (includePooledGo)
			{
				List<string> keys = this.__goPool.Keys.ToList();
				for (int i = keys.Count - 1; i >= 0; i--)
				{
					var path = keys[i];
					if (patharray != null && dict_path.ContainsKey(path) && this.__goPool.TryOnlyGet(path, out var pooledGo))
					{
						if (pooledGo != null && this.__CheckNeedUnload(path))
						{
							ResourcesManager.Instance.ReleaseAsset(pooledGo);
							this.__goPool.Remove(path);
						}
					}
				}
			}
		}
		
		/// <summary>
		/// 获取已经缓存的预制
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public GameObject GetCachedGoWithPath(string path)
		{
			if (this.__goPool.TryOnlyGet(path, out var res))
			{
				return res;
			}
			return null;
		}
		
				
		#region 私有方法
		        
		/// <summary>
		/// 初始化inst
		/// </summary>
		/// <param name="inst"></param>
		void InitInst(GameObject inst)
		{
			if (inst != null)
			{
				inst.SetActive(true);
			}
		}
		/// <summary>
		/// 检测是否已经被缓存
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		bool CheckHasCached(string path)
		{
			if (string.IsNullOrEmpty(path))
			{
				Log.Error("path err :\"" + path + "\"");
				return false;
			}
			if (!path.EndsWith(".prefab"))
			{
				Log.Error("GameObject must be prefab : \"" + path + "\"");
				return false;
			}

			if (this.__instCache.ContainsKey(path) && this.__instCache[path].Count > 0)
			{
				return true;
			}
			return this.__goPool.ContainsKey(path);
		}

		/// <summary>
		/// 缓存并实例化GameObject
		/// </summary>
		/// <param name="path"></param>
		/// <param name="req"></param>
		/// <param name="inst_count"></param>
		void CacheAndInstGameObject(string path, GameObject go, int inst_count)
		{
			this.__goPool.Set(path, go);
			this.__InitGoChildCount(path, go);
			if (inst_count > 0)
			{
				List<GameObject> cachedInst;
				if (!this.__instCache.TryGetValue(path, out cachedInst))
					cachedInst = new List<GameObject>();
				for (int i = 0; i < inst_count; i++)
				{
					var inst = GameObject.Instantiate(go);
					inst.transform.SetParent(this.__cacheTransRoot);
					inst.SetActive(false);
					cachedInst.Add(inst);
					this.__instPathCache[inst] = path;
				}
				this.__instCache[path] = cachedInst;
				if (!this.__goInstCountCache.ContainsKey(path)) this.__goInstCountCache[path] = 0;
				this.__goInstCountCache[path] = this.__goInstCountCache[path] + inst_count;
			}
		}
		/// <summary>
		/// 删除gameobject 所有从GameObjectPool中
		/// </summary>
		/// <param name="inst"></param>
		void DestroyGameObject(GameObject inst)
		{
			if (this.__instPathCache.TryGetValue(inst, out string path))
			{
				if (this.__goInstCountCache.TryGetValue(path, out int count))
				{
					if (count <= 0)
					{
						Log.Error("__goInstCountCache[path] must > 0");
					}
					else
					{
						this.__CheckRecycleInstIsDirty(path, inst, () =>
						{
							GameObject.Destroy(inst);
							this.__goInstCountCache[path] = this.__goInstCountCache[path] - 1;
						});
					}
				}
			}
			else
			{
				Log.Error("DestroyGameObject inst not found from __instPathCache");
			}
		}
		/// <summary>
		/// 检查回收时是否污染
		/// </summary>
		/// <param name="path"></param>
		/// <param name="inst"></param>
		/// <param name="callback"></param>
		void __CheckRecycleInstIsDirty(string path, GameObject inst, Action callback)
		{
			if (!this.__IsOpenCheck())
			{
				callback?.Invoke();
				return;
			}
			inst.SetActive(false);
			this.__CheckAfter(path, inst).Coroutine();
			callback?.Invoke();
		}
	    /// <summary>
	    /// 延迟一段时间检查
	    /// </summary>
	    /// <param name="path"></param>
	    /// <param name="inst"></param>
	    /// <returns></returns>
	    async ETTask __CheckAfter(string path, GameObject inst)
		{
			await TimerManager.Instance.WaitAsync(2000);
			if (inst != null && inst.transform != null && this.__CheckInstIsInPool(path, inst))
			{
				var go_child_count = this.__goChildsCountPool[path];
				Dictionary<string, int> childsCountMap = new Dictionary<string, int>();
				int inst_child_count = this.RecursiveGetChildCount(inst.transform, "", ref childsCountMap);
				if (go_child_count != inst_child_count)
				{
					Log.Error($"go_child_count({ go_child_count }) must equip inst_child_count({inst_child_count}) path = {path} ");
					foreach (var item in childsCountMap)
					{
						var k = item.Key;
						var v = item.Value;
						var unfair = false;
						if (!this.__detailGoChildsCount[path].ContainsKey(k))
							unfair = true;
						else if (this.__detailGoChildsCount[path][k] != v)
							unfair = true;
						if (unfair)
							Log.Error($"not match path on checkrecycle = { k}, count = {v}");
					}
				}
			}
		}
		/// <summary>
		/// 检查inst是否在池子中
		/// </summary>
		/// <param name="path"></param>
		/// <param name="inst"></param>
		/// <returns></returns>
		bool __CheckInstIsInPool(string path, GameObject inst)
		{
			if (this.__instCache.TryGetValue(path, out var inst_array))
			{
				for (int i = 0; i < inst_array.Count; i++)
				{
					if (inst_array[i] == inst) return true;
				}
			}
			return false;
		}
		/// <summary>
		/// 获取GameObject的child数量
		/// </summary>
		/// <param name="path"></param>
		/// <param name="go"></param>
		void __InitGoChildCount(string path, GameObject go)
		{
			if (!this.__IsOpenCheck()) return;
			if (!this.__goChildsCountPool.ContainsKey(path))
			{
				Dictionary<string, int> childsCountMap = new Dictionary<string, int>();
				int total_child_count = this.RecursiveGetChildCount(go.transform, "", ref childsCountMap);
				this.__goChildsCountPool[path] = total_child_count;
				this.__detailGoChildsCount[path] = childsCountMap;
			}
		}

		/// <summary>
		/// 释放资源
		/// </summary>
		/// <param name="path"></param>
		public void __ReleaseAsset(string path)
		{
			if (this.__instCache.ContainsKey(path))
			{
				for (int i = this.__instCache[path].Count - 1; i >= 0; i--)
				{
					this.__instPathCache.Remove(this.__instCache[path][i]);
					GameObject.Destroy(this.__instCache[path][i]);
					this.__instCache[path].RemoveAt(i);
				}
				this.__instCache.Remove(path);
				this.__goInstCountCache.Remove(path);
			}
			if (this.__goPool.TryOnlyGet(path, out var pooledGo) && this.__CheckNeedUnload(path))
			{
				ResourcesManager.Instance.ReleaseAsset(pooledGo);
				this.__goPool.Remove(path);
			}
		}
		/// <summary>
		/// 是否开启检查污染
		/// </summary>
		/// <returns></returns>
		bool __IsOpenCheck()
		{
			return Define.Debug;
		}

		/// <summary>
		/// 递归取子节点数量
		/// </summary>
		/// <param name="trans"></param>
		/// <param name="path"></param>
		/// <param name="record"></param>
		/// <returns></returns>
		int RecursiveGetChildCount(Transform trans, string path, ref Dictionary<string, int> record)
		{
			int total_child_count = trans.childCount;
			for (int i = 0; i < trans.childCount; i++)
			{
				var child = trans.GetChild(i);
				if (child.name.Contains("Input Caret") || child.name.Contains("TMP SubMeshUI") || child.name.Contains("TMP UI SubObject") || /*child.GetComponent<LoopListViewItem2>()!=null
					 || child.GetComponent<LoopGridViewItem>() != null ||*/ (child.name.Contains("Caret") && child.parent.name.Contains("Text Area")))
				{
					//Input控件在运行时会自动生成个光标子控件，而prefab中是没有的，所以得过滤掉
					//TextMesh会生成相应字体子控件
					//TextMeshInput控件在运行时会自动生成个光标子控件，而prefab中是没有的，所以得过滤掉
					total_child_count = total_child_count - 1;
				}
				else
				{
					string cpath = path + "/" + child.name;
					if (record.ContainsKey(cpath))
					{
						record[cpath] += 1;
					}
					else
					{
						record[cpath] = 1;
					}
					total_child_count += this.RecursiveGetChildCount(child, cpath, ref record);
				}
			}
			return total_child_count;
		}
		
		/// <summary>
		/// 检查指定路径是否有未回收的预制体
		/// </summary>
		/// <param name="path"></param>
		private bool __CheckNeedUnload(string path)
		{
			return !this.__instPathCache.ContainsValue(path);
		}
		
		#endregion
		
    }
}