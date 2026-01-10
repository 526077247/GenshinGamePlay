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
    public class GameObjectPoolManager:IManager,IManager<string>
    {
	    public string PackageName { get; private set; } = Define.DefaultName;
	    private static Dictionary<string, GameObjectPoolManager> instances = new Dictionary<string, GameObjectPoolManager>();

        private Transform cacheTransRoot;


        private LruCache<string, GameObject> goPool;
        private Dictionary<string, int> goInstCountCache;//go: inst count 用于记录go产生了多少个实例

        private Dictionary<string, int> goChildrenCountPool;//path: child count 用于在editor模式下检测回收的go是否被污染 path:num

        private Dictionary<string, List<GameObject>> instCache; //path: inst array
        private Dictionary<GameObject, string> instPathCache;// inst : prefab path 用于销毁和回收时反向找到inst对应的prefab TODO:这里有优化空间path太占内存
        private HashSet<string> persistentPathCache;//需要持久化的资源
        private Dictionary<string, Dictionary<string, int>> detailGoChildrenCount;//记录go子控件具体数量信息

        public static GameObjectPoolManager GetInstance(string package = null)
        {
	        if (package == null) package = Define.DefaultName;
	        if (!instances.TryGetValue(package, out var mgr))
	        {
		        mgr = ManagerProvider.RegisterManager<GameObjectPoolManager,string>(package,package);
		        instances.Add(package, mgr);
	        }
	        return mgr;
        }

        #region override

        public void Init(string name)
        {
	        Init();
	        PackageName = name;
        }

        public void Init()
        {
	        PackageName = Define.DefaultName;
	        goPool = new LruCache<string, GameObject>();
            goInstCountCache = new Dictionary<string, int>();
            goChildrenCountPool = new Dictionary<string, int>();
            instCache = new Dictionary<string, List<GameObject>>();
            instPathCache = new Dictionary<GameObject, string>();
            persistentPathCache = new HashSet<string>();
            detailGoChildrenCount = new Dictionary<string, Dictionary<string, int>>();

            var go = GameObject.Find("GameObjectCacheRoot");
            if (go == null)
            {
                go = new GameObject("GameObjectCacheRoot");
            }
            GameObject.DontDestroyOnLoad(go);
            cacheTransRoot = go.transform;

            goPool.SetPopCallback((path, pooledGo) =>
            {
                ReleaseAsset(path);
            });
            goPool.SetCheckCanPopCallback((path, pooledGo) =>
            {
                var cnt = goInstCountCache[path] - (instCache.ContainsKey(path) ? instCache[path].Count : 0);
                if (cnt > 0)
	                Log.Info(
		                $"path={path} goInstCountCache={goInstCountCache[path]} instCache={(instCache[path] != null ? instCache[path].Count : 0)}");
                return cnt == 0 && !persistentPathCache.Contains(path);
            });
        }

        public void Destroy()
        {
	        Cleanup();
	        if (PackageName != null)
	        {
		        if (instances != null && instances.ContainsKey(PackageName))
		        {
			        instances.Remove(PackageName);
		        }

		        PackageName = null;
	        }
        }
        
        #endregion


        /// <summary>
		/// 预加载一系列资源
		/// </summary>
        /// <param name="res"></param>
		public async ETTask LoadDependency(List<string> res)
		{
			if (res.Count <= 0) return;
			using (ListComponent<ETTask> tasks = ListComponent<ETTask>.Create())
			{
				for (int i = 0; i < res.Count; i++)
				{
					tasks.Add(PreLoadGameObjectAsync(res[i], 1));
				}
				await ETTaskHelper.WaitAll(tasks);
			}
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
				if (CheckHasCached(path))
				{
					callback?.Invoke();
				}
				else
				{
					string package = PackageName;
					//特殊处理，Default和Resources一起管理
					if (package == Define.DefaultName && path.StartsWith(Define.ResourcesName))
					{
						package = Define.ResourcesName;
					}
					var go = await ResourcesManager.Instance.LoadAsync<GameObject>(path, package: package);
					if (go != null)
					{
						CacheAndInstGameObject(path, go, instCount);
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
			GetGameObjectAsync(path, (data) =>
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
			if (TryGetFromCache(path, out var inst))
			{
				InitInst(inst);
				callback?.Invoke(inst);
				return inst;
			}
			await PreLoadGameObjectAsync(path, 1);
			if (TryGetFromCache(path, out inst))
			{
				InitInst(inst);
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
			if (TryGetFromCache(path, out var inst))
			{
				InitInst(inst);
				return inst;
			}
			return null;
		}
		/// <summary>
		/// 回收
		/// </summary>
		/// <param name="inst"></param>
		/// <param name="clearLimit">现有缓存达到多少开始销毁，-1表示不销毁</param>
		public void RecycleGameObject(GameObject inst, int clearLimit = -1)
		{
			if (!instPathCache.ContainsKey(inst))
			{
				Log.Error("RecycleGameObject inst not found from instPathCache");
				GameObject.Destroy(inst);
				return;
			}
			var path = instPathCache[inst];
			int count = 0;
			if (instCache.TryGetValue(path, out var list))
			{
				count = list.Count;
			}
			if (clearLimit < 0 || clearLimit > count)
			{
				CheckRecycleInstIsDirty(path, inst, null);
				inst.transform.SetParent(cacheTransRoot, false);
				inst.SetActive(false);
				if (list == null)
				{
					instCache[path] = new List<GameObject>();
				}
				instCache[path].Add(inst);
			}
			else
			{
				DestroyGameObject(inst);
			}

			//CheckCleanRes(path);
		}

		/// <summary>
		/// <para>检测回收的时候是否需要清理资源(这里是检测是否清空 inst和缓存的go)</para>
		/// <para>这里可以考虑加一个配置表来处理优先级问题，一些优先级较高的保留</para>
		/// </summary>
		/// <param name="path"></param>
		public void CheckCleanRes(string path)
		{
			var cnt = goInstCountCache[path] - (instCache.ContainsKey(path) ? instCache[path].Count : 0);
			if (cnt == 0 && !persistentPathCache.Contains(path))
				ReleaseAsset(path);
		}

		/// <summary>
		/// <para></para>
		/// </summary>
		/// <param name="path"></param>
		public void AddPersistentPrefabPath(string path)
		{
			persistentPathCache.Add(path);

		}
		
		/// <summary>
		/// <para>清理缓存</para>
		/// </summary>
		/// <param name="includePooledGo">是否需要将预设也释放</param>
		/// <param name="ignorePathArray">忽略的</param>
		public void Cleanup(bool includePooledGo = true, List<string> ignorePathArray = null)
		{
			Log.Info("GameObjectPool Cleanup "+PackageName);
			HashSetComponent<string> ignorePath = null;
			if (ignorePathArray != null)
			{
				ignorePath = HashSetComponent<string>.Create();
				for (int i = 0; i < ignorePathArray.Count; i++)
				{
					ignorePath.Add(ignorePathArray[i]);
				}
			}
			foreach (var item in instCache)
			{
				if (ignorePath != null && ignorePath.Contains(item.Key)) continue;
				for (int i = 0; i < item.Value.Count; i++)
				{
					var inst = item.Value[i];
					if (inst != null)
					{
						GameObject.Destroy(inst);
						goInstCountCache[item.Key]--;
						if (goInstCountCache[item.Key] == 0)
						{
							goInstCountCache.Remove(item.Key);
						}
					}
					instPathCache.Remove(inst);
				}
				item.Value.Clear();
			}

			if (includePooledGo)
			{
				List<string> keys = goPool.Keys.ToList();
				for (int i = keys.Count - 1; i >= 0; i--)
				{
					var path = keys[i];
					if (ignorePath != null && ignorePath.Contains(path)) continue;
					if (goPool.TryOnlyGet(path, out var pooledGo) 
					    && !persistentPathCache.Contains(path) &&
					    pooledGo != null && CheckNeedUnload(path))
					{
						ResourcesManager.Instance.ReleaseAsset(pooledGo);
						goPool.Remove(path);
					}

				}
			}
			ignorePath?.Dispose();
			Log.Info("GameObjectPool Cleanup Over"+PackageName);
		}

		/// <summary>
		/// <para>释放asset(包括指定为持久化的资源)</para>
		/// <para>注意这里需要保证外面没有引用这些path的inst了，不然会出现材质丢失的问题</para>
		/// <para>不要轻易调用，除非你对内部的资源的生命周期有了清晰的了解</para>
		/// </summary>
		/// <param name="releasePathArray">需要释放的资源路径数组</param>
		/// <param name="includePooledGo">是否需要将预设也释放</param>
		public void CleanupWithPathArray(List<string> releasePathArray, bool includePooledGo = true)
		{
			if (releasePathArray == null || releasePathArray.Count == 0) return;
			Log.Info("GameObjectPool Cleanup ");
			using (HashSetComponent<string> dictPath = HashSetComponent<string>.Create())
			{
				for (int i = 0; i < releasePathArray.Count; i++)
				{
					dictPath.Add(releasePathArray[i]);
				}

				foreach (var item in instCache)
				{
					if (dictPath.Contains(item.Key))
					{
						for (int i = 0; i < item.Value.Count; i++)
						{
							var inst = item.Value[i];
							if (inst != null)
							{
								GameObject.Destroy(inst);
								goInstCountCache[item.Key]--;
								if (goInstCountCache[item.Key] == 0)
								{
									goInstCountCache.Remove(item.Key);
								}
							}

							instPathCache.Remove(inst);
						}
					}
				}

				for (int i = 0; i < releasePathArray.Count; i++)
				{
					instCache.Remove(releasePathArray[i]);
				}

				if (includePooledGo)
				{
					List<string> keys = goPool.Keys.ToList();
					for (int i = keys.Count - 1; i >= 0; i--)
					{
						var path = keys[i];
						if (dictPath.Contains(path)
						    && goPool.TryOnlyGet(path, out var pooledGo)
						    && pooledGo != null && CheckNeedUnload(path))
						{
							ResourcesManager.Instance.ReleaseAsset(pooledGo);
							goPool.Remove(path);
							persistentPathCache.Remove(path);
						}
					}
				}
			}
		}
		
		#region 私有方法
		        
		/// <summary>
		/// 尝试从缓存中获取
		/// </summary>
		/// <param name="path"></param>
		/// <param name="go"></param>
		/// <returns></returns>
		private bool TryGetFromCache(string path, out GameObject go)
		{
			go = null;
			if (!CheckHasCached(path)) return false;
			if (instCache.TryGetValue(path, out var cachedInst))
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
			if (goPool.TryGet(path, out var pooledGo))
			{
				if (pooledGo != null)
				{
					var inst = GameObject.Instantiate(pooledGo);
					if(goInstCountCache.ContainsKey(path))
						goInstCountCache[path]++;
					else 
						goInstCountCache[path] = 1;
					instPathCache[inst] = path;
					go = inst;
					return true;
				}
			}
			return false;
		}
		
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

			if (instCache.ContainsKey(path) && instCache[path].Count > 0)
			{
				return true;
			}
			return goPool.ContainsKey(path);
		}

		/// <summary>
		/// 缓存并实例化GameObject
		/// </summary>
		/// <param name="path"></param>
		/// <param name="go"></param>
		/// <param name="instCount"></param>
		void CacheAndInstGameObject(string path, GameObject go, int instCount)
		{
			goPool.Set(path, go);
			InitGoChildCount(path, go);
			if (instCount > 0)
			{
				if (!instCache.TryGetValue(path, out var cachedInst))
				{
					instCache[path] = cachedInst = new List<GameObject>();
				}
				for (int i = 0; i < instCount; i++)
				{
					var inst = GameObject.Instantiate(go);
					inst.transform.SetParent(cacheTransRoot);
					inst.SetActive(false);
					cachedInst.Add(inst);
					instPathCache[inst] = path;
				}

				if (!goInstCountCache.ContainsKey(path))
				{
					goInstCountCache[path] = instCount;
				}
				else
				{
					goInstCountCache[path] += instCount;
				}
			}
		}
		/// <summary>
		/// 删除GameObject
		/// </summary>
		/// <param name="inst"></param>
		void DestroyGameObject(GameObject inst)
		{
			if (instPathCache.TryGetValue(inst, out string path))
			{
				if (goInstCountCache.TryGetValue(path, out int count))
				{
					if (count <= 0)
					{
						Log.Error("goInstCountCache[path] must > 0");
					}
					else
					{
						CheckRecycleInstIsDirty(path, inst, () =>
						{
							GameObject.Destroy(inst);
							goInstCountCache[path] --;
							if (goInstCountCache[path] == 0)
							{
								goInstCountCache.Remove(path);
							}
						});
						instPathCache.Remove(inst);
					}
				}
			}
			else
			{
				Log.Error("DestroyGameObject inst not found from instPathCache");
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
			if (instCache.TryGetValue(path, out var instArray))
			{
				for (int i = 0; i < instArray.Count; i++)
				{
					if (instArray[i] == inst) return true;
				}
			}
			return false;
		}
		
		/// <summary>
		/// 释放资源
		/// </summary>
		/// <param name="path"></param>
		private void ReleaseAsset(string path)
		{
			if (instCache.ContainsKey(path))
			{
				for (int i = instCache[path].Count - 1; i >= 0; i--)
				{
					instPathCache.Remove(instCache[path][i]);
					GameObject.Destroy(instCache[path][i]);
					instCache[path].RemoveAt(i);
				}
				instCache.Remove(path);
				goInstCountCache.Remove(path);
			}
			if (goPool.TryOnlyGet(path, out var pooledGo) && CheckNeedUnload(path))
			{
				ResourcesManager.Instance.ReleaseAsset(pooledGo);
				goPool.Remove(path);
			}
		}

		/// <summary>
		/// 检查指定路径是否已经没有未回收的实例
		/// </summary>
		/// <param name="path"></param>
		private bool CheckNeedUnload(string path)
		{
			return !instPathCache.ContainsValue(path);
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
		/// 获取GameObject的child数量
		/// </summary>
		/// <param name="path"></param>
		/// <param name="go"></param>
		void InitGoChildCount(string path, GameObject go)
		{
			if (!IsOpenCheck()) return;
			if (!goChildrenCountPool.ContainsKey(path))
			{
				Dictionary<string, int> childsCountMap = new Dictionary<string, int>();
				int totalChildCount = RecursiveGetChildCount(go.transform, "", childsCountMap);
				goChildrenCountPool[path] = totalChildCount;
				detailGoChildrenCount[path] = childsCountMap;
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
			if (!IsOpenCheck())
			{
				callback?.Invoke();
				return;
			}
			inst.SetActive(false);
			CheckAfter(path, inst).Coroutine();
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
			if (inst != null && inst.transform != null && CheckInstIsInPool(path, inst))
			{
				var goChildCount = goChildrenCountPool[path];
				Dictionary<string, int> childsCountMap = new Dictionary<string, int>();
				int instChildCount = RecursiveGetChildCount(inst.transform, "", childsCountMap);
				if (goChildCount != instChildCount)
				{
					Log.Error($"go child count({ goChildCount }) must equip inst child count({instChildCount}) path = {path} ");
					foreach (var item in childsCountMap)
					{
						var k = item.Key;
						var v = item.Value;
						var unfair = false;
						if (!detailGoChildrenCount[path].ContainsKey(k))
							unfair = true;
						else if (detailGoChildrenCount[path][k] != v)
							unfair = true;
						if (unfair)
							Log.Error($"not match path on checkrecycle = { k}, count = {v}");
					}
				}
			}
		}

		/// <summary>
		/// 递归取子节点数量
		/// </summary>
		/// <param name="trans"></param>
		/// <param name="path"></param>
		/// <param name="record"></param>
		/// <returns></returns>
		int RecursiveGetChildCount(Transform trans, string path, Dictionary<string, int> record)
		{
			int totalChildCount = trans.childCount;
			for (int i = 0; i < trans.childCount; i++)
			{
				var child = trans.GetChild(i);
				if (child.name.Contains("Input Caret") || child.name.Contains("TMP SubMeshUI") || child.name.Contains("TMP UI SubObject") 
				    || child.GetComponent<SuperScrollView.LoopListViewItem2>()!=null
					 || child.GetComponent<SuperScrollView.LoopGridViewItem>() != null
				    || child.parent.GetComponent<CopyGameObject>() != null
				    || (child.name.Contains("Caret") && child.parent.name.Contains("Text Area")))
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
					totalChildCount += RecursiveGetChildCount(child, cpath, record);
				}
			}
			return totalChildCount;
		}
		#endregion
		
    }
}