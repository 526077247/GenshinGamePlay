using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using YooAsset;

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
	    public string PackageName { get;private set; }
	    private static Dictionary<string, GameObjectPoolManager> instances;

        private Transform cacheTransRoot;


        private LruCache<string, GameObject> goPool;
        private Dictionary<string, int> goInstCountCache;//go: inst_count 用于记录go产生了多少个实例

        private Dictionary<string, int> goChildsCountPool;//path: child_count 用于在editor模式下检测回收的go是否被污染 path:num

        private Dictionary<string, List<GameObject>> instCache; //path: inst_array
        private Dictionary<GameObject, string> instPathCache;// inst : prefab_path 用于销毁和回收时反向找到inst对应的prefab TODO:这里有优化空间path太占内存
        private Dictionary<string, bool> persistentPathCache;//需要持久化的资源
        private Dictionary<string, Dictionary<string, int>> detailGoChildsCount;//记录go子控件具体数量信息

        public static GameObjectPoolManager GetInstance(string package = null)
        {
	        if (package == null) package = YooAssetsMgr.DefaultName;
	        if (instances == null)
	        {
		        instances = new Dictionary<string, GameObjectPoolManager>();
	        }
	        if (!instances.TryGetValue(package, out var mgr))
	        {
		        mgr = ManagerProvider.RegisterManager<GameObjectPoolManager>(package);
		        mgr.PackageName = package;
		        instances.Add(package, mgr);
	        }
	        return mgr;
        }

        #region override
        
        public void Init()
        {
	        this.goPool = new LruCache<string, GameObject>();
            this.goInstCountCache = new Dictionary<string, int>();
            this.goChildsCountPool = new Dictionary<string, int>();
            this.instCache = new Dictionary<string, List<GameObject>>();
            this.instPathCache = new Dictionary<GameObject, string>();
            this.persistentPathCache = new Dictionary<string, bool>();
            this.detailGoChildsCount = new Dictionary<string, Dictionary<string, int>>();

            var go = GameObject.Find("GameObjectCacheRoot");
            if (go == null)
            {
                go = new GameObject("GameObjectCacheRoot");
            }
            GameObject.DontDestroyOnLoad(go);
            this.cacheTransRoot = go.transform;

            this.goPool.SetPopCallback((path, pooledGo) =>
            {
                this.ReleaseAsset(path);
            });
            this.goPool.SetCheckCanPopCallback((path, pooledGo) =>
            {
                var cnt = this.goInstCountCache[path] - (this.instCache.ContainsKey(path) ? this.instCache[path].Count : 0);
                if (cnt > 0)
                    Log.Info(string.Format("path={0} goInstCountCache={1} instCache={2}", path, this.goInstCountCache[path], 
                        (this.instCache[path] != null ? this.instCache[path].Count : 0)));
                return cnt == 0 && !this.persistentPathCache.ContainsKey(path);
            });
        }

        public void Destroy()
        {
	        this.Cleanup();
	        if (instances.ContainsKey(PackageName))
	        {
		        instances.Remove(PackageName);
		        if (instances.Count <= 0) instances = null;
	        }

	        PackageName = null;
        }
        
        #endregion


        /// <summary>
		/// 预加载一系列资源
		/// </summary>
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
			if (this.instCache.TryGetValue(path, out var cachedInst))
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
			if (this.goPool.TryGet(path, out var pooledGo))
			{
				if (pooledGo != null)
				{
					var inst = GameObject.Instantiate(pooledGo);
					this.goInstCountCache[path]++;
					this.instPathCache[inst] = path;
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
		/// <param name="instCount">初始实例化个数</param>
		/// <param name="callback"></param>
		public async ETTask PreLoadGameObjectAsync(string path, int instCount,Action callback = null)
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
					var go = await ResourcesManager.Instance.LoadAsync<GameObject>(path,package: PackageName);
					if (go != null)
					{
						this.CacheAndInstGameObject(path, go as GameObject, instCount);
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
		public GameObject GetGameObjectFromPool(string path)
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
		/// <param name="isClear">是否忽略污染检查</param>
		public void RecycleGameObject(GameObject inst, bool isClear = false)
		{
			if (!this.instPathCache.ContainsKey(inst))
			{
				Log.Error("RecycleGameObject inst not found from instPathCache");
				return;
			}
			var path = this.instPathCache[inst];
			if (!isClear)
			{
				this.CheckRecycleInstIsDirty(path, inst, null);
				inst.transform.SetParent(this.cacheTransRoot, false);
				inst.SetActive(false);
				if (!this.instCache.ContainsKey(path))
				{
					this.instCache[path] = new List<GameObject>();
				}
				this.instCache[path].Add(inst);
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
			var cnt = this.goInstCountCache[path] - (this.instCache.ContainsKey(path) ? this.instCache[path].Count : 0);
			if (cnt == 0 && !this.persistentPathCache.ContainsKey(path))
				this.ReleaseAsset(path);
		}

		/// <summary>
		/// <para>添加需要持久化的资源</para>
		/// </summary>
		/// <param name="path"></param>
		public void AddPersistentPrefabPath(string path)
		{
			this.persistentPathCache[path] = true;

		}
		
		/// <summary>
		/// <para>清理缓存</para>
		/// </summary>
		/// <param name="includePooledGo">是否需要将预设也释放</param>
		/// <param name="excludePathArray">忽略的</param>
		public void Cleanup(bool includePooledGo = true, List<string> excludePathArray = null)
		{
			Log.Info("GameObjectPool Cleanup ");
			foreach (var item in this.instCache)
			{
				for (int i = 0; i < item.Value.Count; i++)
				{
					var inst = item.Value[i];
					if (inst != null)
					{
						GameObject.Destroy(inst);
						this.goInstCountCache[item.Key]--;
					}
					this.instPathCache.Remove(inst);
				}
			}
			this.instCache = new Dictionary<string, List<GameObject>>();

			if (includePooledGo)
			{
				Dictionary<string, bool> dictExcludePath = null;
				if (excludePathArray != null)
				{
					dictExcludePath = new Dictionary<string, bool>();
					for (int i = 0; i < excludePathArray.Count; i++)
					{
						dictExcludePath[excludePathArray[i]] = true;
					}
				}

				List<string> keys = this.goPool.Keys.ToList();
				for (int i = keys.Count - 1; i >= 0; i--)
				{
					var path = keys[i];
					if (dictExcludePath != null && !dictExcludePath.ContainsKey(path) && this.goPool.TryOnlyGet(path, out var pooledGo))
					{
						if (pooledGo != null && this.CheckNeedUnload(path))
						{
							ResourcesManager.Instance.ReleaseAsset(pooledGo);
							this.goPool.Remove(path);
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
		/// <param name="releasePathArray">需要释放的资源路径数组</param>
		public void CleanupWithPathArray(bool includePooledGo = true, List<string> releasePathArray = null)
		{
			Debug.Log("GameObjectPool Cleanup ");
			Dictionary<string, bool> dictPath = null;
			if (releasePathArray != null)
			{
				dictPath = new Dictionary<string, bool>();
				for (int i = 0; i < releasePathArray.Count; i++)
				{
					dictPath[releasePathArray[i]] = true;
				}
				foreach (var item in this.instCache)
				{
					if (dictPath.ContainsKey(item.Key))
					{
						for (int i = 0; i < item.Value.Count; i++)
						{
							var inst = item.Value[i];
							if (inst != null)
							{
								GameObject.Destroy(inst);
								this.goInstCountCache[item.Key]-- ;
							}
							this.instPathCache.Remove(inst);
						}
					}
				}
				for (int i = 0; i < releasePathArray.Count; i++)
				{
					this.instCache.Remove(releasePathArray[i]);
				}
			}

			if (includePooledGo)
			{
				List<string> keys = goPool.Keys.ToList();
				for (int i = keys.Count - 1; i >= 0; i--)
				{
					var path = keys[i];
					if (releasePathArray != null && dictPath.ContainsKey(path) && goPool.TryOnlyGet(path, out var pooledGo))
					{
						if (pooledGo != null && CheckNeedUnload(path))
						{
							ResourcesManager.Instance.ReleaseAsset(pooledGo);
							goPool.Remove(path);
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
			if (this.goPool.TryOnlyGet(path, out var res))
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

			if (this.instCache.ContainsKey(path) && this.instCache[path].Count > 0)
			{
				return true;
			}
			return this.goPool.ContainsKey(path);
		}

		/// <summary>
		/// 缓存并实例化GameObject
		/// </summary>
		/// <param name="path"></param>
		/// <param name="go"></param>
		/// <param name="instCount"></param>
		void CacheAndInstGameObject(string path, GameObject go, int instCount)
		{
			this.goPool.Set(path, go);
			this.InitGoChildCount(path, go);
			if (instCount > 0)
			{
				List<GameObject> cachedInst;
				if (!this.instCache.TryGetValue(path, out cachedInst))
					cachedInst = new List<GameObject>();
				for (int i = 0; i < instCount; i++)
				{
					var inst = GameObject.Instantiate(go);
					inst.transform.SetParent(this.cacheTransRoot);
					inst.SetActive(false);
					cachedInst.Add(inst);
					this.instPathCache[inst] = path;
				}
				this.instCache[path] = cachedInst;
				if (!this.goInstCountCache.ContainsKey(path)) this.goInstCountCache[path] = 0;
				this.goInstCountCache[path] += instCount;
			}
		}
		/// <summary>
		/// 删除gameobject
		/// </summary>
		/// <param name="inst"></param>
		void DestroyGameObject(GameObject inst)
		{
			if (this.instPathCache.TryGetValue(inst, out string path))
			{
				if (this.goInstCountCache.TryGetValue(path, out int count))
				{
					if (count <= 0)
					{
						Log.Error("goInstCountCache[path] must > 0");
					}
					else
					{
						this.CheckRecycleInstIsDirty(path, inst, () =>
						{
							GameObject.Destroy(inst);
							this.goInstCountCache[path] --;
						});
					}
				}
			}
			else
			{
				Log.Error("DestroyGameObject inst not found from instPathCache");
			}
		}
		/// <summary>
		/// 检查回收时是否污染
		/// </summary>
		/// <param name="path"></param>
		/// <param name="inst"></param>
		/// <param name="callback"></param>
		void CheckRecycleInstIsDirty(string path, GameObject inst, Action callback)
		{
			if (!this.IsOpenCheck())
			{
				callback?.Invoke();
				return;
			}
			inst.SetActive(false);
			this.CheckAfter(path, inst).Coroutine();
			callback?.Invoke();
		}
	    /// <summary>
	    /// 延迟一段时间检查
	    /// </summary>
	    /// <param name="path"></param>
	    /// <param name="inst"></param>
	    /// <returns></returns>
	    async ETTask CheckAfter(string path, GameObject inst)
		{
			await TimerManager.Instance.WaitAsync(2000);
			if (inst != null && inst.transform != null && this.CheckInstIsInPool(path, inst))
			{
				var goChildCount = this.goChildsCountPool[path];
				Dictionary<string, int> childsCountMap = new Dictionary<string, int>();
				int instChildCount = this.RecursiveGetChildCount(inst.transform, "", ref childsCountMap);
				if (goChildCount != instChildCount)
				{
					Log.Error($"go_child_count({ goChildCount }) must equip inst_child_count({instChildCount}) path = {path} ");
					foreach (var item in childsCountMap)
					{
						var k = item.Key;
						var v = item.Value;
						var unfair = false;
						if (!this.detailGoChildsCount[path].ContainsKey(k))
							unfair = true;
						else if (this.detailGoChildsCount[path][k] != v)
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
		bool CheckInstIsInPool(string path, GameObject inst)
		{
			if (this.instCache.TryGetValue(path, out var instArray))
			{
				for (int i = 0; i < instArray.Count; i++)
				{
					if (instArray[i] == inst) return true;
				}
			}
			return false;
		}
		/// <summary>
		/// 获取GameObject的child数量
		/// </summary>
		/// <param name="path"></param>
		/// <param name="go"></param>
		void InitGoChildCount(string path, GameObject go)
		{
			if (!this.IsOpenCheck()) return;
			if (!this.goChildsCountPool.ContainsKey(path))
			{
				Dictionary<string, int> childsCountMap = new Dictionary<string, int>();
				int totalChildCount = this.RecursiveGetChildCount(go.transform, "", ref childsCountMap);
				this.goChildsCountPool[path] = totalChildCount;
				this.detailGoChildsCount[path] = childsCountMap;
			}
		}

		/// <summary>
		/// 释放资源
		/// </summary>
		/// <param name="path"></param>
		public void ReleaseAsset(string path)
		{
			if (this.instCache.ContainsKey(path))
			{
				for (int i = this.instCache[path].Count - 1; i >= 0; i--)
				{
					this.instPathCache.Remove(this.instCache[path][i]);
					GameObject.Destroy(this.instCache[path][i]);
					this.instCache[path].RemoveAt(i);
				}
				this.instCache.Remove(path);
				this.goInstCountCache.Remove(path);
			}
			if (this.goPool.TryOnlyGet(path, out var pooledGo) && this.CheckNeedUnload(path))
			{
				ResourcesManager.Instance.ReleaseAsset(pooledGo);
				this.goPool.Remove(path);
			}
		}
		/// <summary>
		/// 是否开启检查污染
		/// </summary>
		/// <returns></returns>
		bool IsOpenCheck()
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
			int totalChildCount = trans.childCount;
			for (int i = 0; i < trans.childCount; i++)
			{
				var child = trans.GetChild(i);
				if (child.name.Contains("Input Caret") || child.name.Contains("TMP SubMeshUI") || child.name.Contains("TMP UI SubObject") || child.GetComponent<SuperScrollView.LoopListViewItem2>()!=null
					 || child.GetComponent<SuperScrollView.LoopGridViewItem>() != null || (child.name.Contains("Caret") && child.parent.name.Contains("Text Area")))
				{
					//Input控件在运行时会自动生成个光标子控件，而prefab中是没有的，所以得过滤掉
					//TextMesh会生成相应字体子控件
					//TextMeshInput控件在运行时会自动生成个光标子控件，而prefab中是没有的，所以得过滤掉
					totalChildCount --;
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
					totalChildCount += this.RecursiveGetChildCount(child, cpath, ref record);
				}
			}
			return totalChildCount;
		}
		
		/// <summary>
		/// 检查指定路径是否有未回收的预制体
		/// </summary>
		/// <param name="path"></param>
		private bool CheckNeedUnload(string path)
		{
			return !this.instPathCache.ContainsValue(path);
		}
		
		#endregion
		
    }
}