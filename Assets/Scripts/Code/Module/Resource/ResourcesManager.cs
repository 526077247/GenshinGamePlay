using System.Collections.Generic;
using YooAsset;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TaoTie
{
    /// <summary>
    /// <para>资源管理系统：提供资源加载管理</para>
    /// <para>注意：</para>
    /// <para>1、只提供异步接口，即使内部使用的是同步操作，对外来说只有异步</para>
    /// <para>2、对于串行执行一连串的异步操作，建议使用协程（用同步形式的代码写异步逻辑），回调方式会使代码难读</para>
    /// <para>3、理论上做到逻辑层脚本对AB名字是完全透明的，所有资源只有packagePath的概念，这里对路径进行处理</para>
    /// </summary>
    public class ResourcesManager : IManager, IManager<IPackageFinder>
    {
        public static ResourcesManager Instance { get; private set; }
        private Dictionary<object, AssetHandle> temp;
        private List<AssetHandle> cachedAssetOperationHandles;
        public IPackageFinder packageFinder { get; private set; }
        private HashSet<AssetHandle> loadingOp;

        #region override

        public void Init()
        {
            Instance = this;
            if(packageFinder==null)packageFinder = new DefaultPackageFinder();
            this.temp = new Dictionary<object, AssetHandle>(1024);
            this.cachedAssetOperationHandles = new List<AssetHandle>(1024);
            loadingOp = new HashSet<AssetHandle>();
        }

        public void Init(IPackageFinder finder)
        {
            packageFinder = finder;
            Init();
        }

        public void Destroy()
        {
            Instance = null;
            ClearAssetsCache();
        }

        #endregion

        /// <summary>
        /// 是否有加载任务正在进行
        /// </summary>
        /// <returns></returns>
        public bool IsProcessRunning()
        {
            return this.loadingOp.Count > 0;
        }

        /// <summary>
        /// 异步加载Asset
        /// </summary>
        /// <param name="path"></param>
        /// <param name="callback"></param>
        /// <param name="package"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public ETTask<T> LoadAsync<T>(string path, Action<T> callback = null, string package = null)
            where T : UnityEngine.Object
        {
            ETTask<T> res = ETTask<T>.Create(true);
            if (string.IsNullOrEmpty(path))
            {
                Log.Error("path is empty");
                callback?.Invoke(null);
                res.SetResult(null);
                return res;
            }

            if (package == null)
            {
                package = packageFinder.GetPackageName(path);
            }

            var op = PackageManager.Instance.LoadAssetAsync<T>(path, package);
            if (op == null)
            {
                Log.Error(package + "加载资源前未初始化！" + path);
                return default;
            }

            this.loadingOp.Add(op);
            op.Completed += (op) =>
            {
                var obj = op.AssetObject as T;
                this.loadingOp.Remove(op);
                callback?.Invoke(obj);
                res.SetResult(obj);
                if (obj != null && !this.temp.ContainsKey(obj))
                {
                    this.temp.Add(op.AssetObject, op);
                    cachedAssetOperationHandles.Add(op);
                }
                else
                {
                    op.Release();
                }
            };
            return res;

        }

        /// <summary>
        /// 异步加载Asset，返回ETTask
        /// </summary>
        /// <param name="path"></param>
        /// <param name="callback"></param>
        /// <param name="package"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public ETTask LoadTask<T>(string path, Action<T> callback = null, string package = null)
            where T : UnityEngine.Object
        {
            ETTask task = ETTask.Create(true);
            if (package == null)
            {
                package = packageFinder.GetPackageName(path);
            }

            this.LoadAsync<T>(path, (data) =>
            {
                callback?.Invoke(data);
                task.SetResult();
            }, package).Coroutine();
            return task;
        }

        /// <summary>
        /// 加载场景
        /// </summary>
        /// <param name="path"></param>
        /// <param name="isAdditive"></param>
        /// <param name="package"></param>
        /// <returns></returns>
        public ETTask LoadSceneAsync(string path, bool isAdditive, string package = null)
        {
            ETTask res = ETTask.Create(true);
            if (string.IsNullOrEmpty(path))
            {
                Log.Error("path err : " + path);
                return res;
            }

            if (package == null)
            {
                package = packageFinder.GetPackageName(path);
            }

            var op = PackageManager.Instance.LoadSceneAsync(path,
                isAdditive ? LoadSceneMode.Additive : LoadSceneMode.Single, package);
            if (op == null)
            {
                Log.Error(package + "加载资源前未初始化！" + path);
                return default;
            }

            op.Completed += (op) => { res.SetResult(); };
            return res;
        }


        /// <summary>
        /// 清理资源：切换场景时调用
        /// </summary>
        /// <param name="excludeClearAssets">不需要清除的</param>
        public void ClearAssetsCache(UnityEngine.Object[] excludeClearAssets = null)
        {
            HashSetComponent<AssetHandle> temp = null;
            if (excludeClearAssets != null)
            {
                temp = HashSetComponent<AssetHandle>.Create();
                for (int i = 0; i < excludeClearAssets.Length; i++)
                {
                    temp.Add(this.temp[excludeClearAssets[i]]);
                }
            }

            for (int i = this.cachedAssetOperationHandles.Count - 1; i >= 0; i--)
            {
                if (temp == null || !temp.Contains(this.cachedAssetOperationHandles[i]))
                {
                    this.temp.Remove(this.cachedAssetOperationHandles[i].AssetObject);
                    this.cachedAssetOperationHandles[i].Release();
                    this.cachedAssetOperationHandles.RemoveAt(i);
                }
            }
            PackageManager.Instance.UnloadUnusedAssets();
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        /// <param name="pooledGo"></param>
        public void ReleaseAsset(UnityEngine.Object pooledGo)
        {
            if (this.temp.TryGetValue(pooledGo, out var op))
            {
                op.Release();
                this.temp.Remove(pooledGo);
                this.cachedAssetOperationHandles.Remove(op);
            }
        }

        /// <summary>
        /// 同步加载json配置
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public async ETTask<string> LoadConfigJsonAsync(string path)
        {
            if (string.IsNullOrEmpty(path)) return default;
            path += ".json";
            var file = await LoadAsync<TextAsset>(path);
            try
            {
                var text = file.text;
                ReleaseAsset(file);
                return text;
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }

            return null;
        }
        
        /// <summary>
        /// 同步加载二进制配置
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public async ETTask<byte[]> LoadConfigBytesAsync(string path)
        {
            if (string.IsNullOrEmpty(path)) return default;
            path += ".bytes";
            var file = await LoadAsync<TextAsset>(path);
            try
            {
                var bytes = file.bytes;
                ReleaseAsset(file);
                return bytes;
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
            return null;
        }
    }
}