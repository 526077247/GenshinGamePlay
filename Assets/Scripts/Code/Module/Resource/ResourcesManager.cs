﻿using System.Collections.Generic;
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
    public class ResourcesManager:IManager
    {

        public static ResourcesManager Instance { get;private set; }
        private int loaderCount = 0;
        private Dictionary<object, AssetOperationHandle> Temp;
        private List<AssetOperationHandle> CachedAssetOperationHandles;

        #region override
        
        public void Init()
        {
            Instance = this;
            this.loaderCount = 0;
            this.Temp = new Dictionary<object, AssetOperationHandle>(1024);
            this.CachedAssetOperationHandles = new List<AssetOperationHandle>(1024);
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
            return this.loaderCount > 0;
        }
        /// <summary>
        /// 同步加载Asset
        /// </summary>
        /// <param name="path"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Load<T>(string path) where T: UnityEngine.Object
        {
            if (string.IsNullOrEmpty(path))
            {
                Log.Error("path is empty");
                return null;
            }
            this.loaderCount++;
            var op = YooAssets.LoadAssetSync<T>(path);
            this.loaderCount--;
            T obj = op.AssetObject as T;
            if (!this.Temp.ContainsKey(op.AssetObject))
            {
                this.Temp.Add(op.AssetObject, op);
            }
            else
            {
                op.Release();
            }
            return obj;

        }
        /// <summary>
        /// 异步加载Asset
        /// </summary>
        /// <param name="path"></param>
        /// <param name="callback"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public ETTask<T> LoadAsync<T>(string path, Action<T> callback = null) where T: UnityEngine.Object
        {
            ETTask<T> res = ETTask<T>.Create(true);
            if (string.IsNullOrEmpty(path))
            {
                Log.Error("path is empty");
                callback?.Invoke(null);
                res.SetResult(null);
                return res;
            }
            this.loaderCount++;
            var op = YooAssets.LoadAssetAsync<T>(path);
            op.Completed += (op) =>
            {
                this.loaderCount--;
                callback?.Invoke(op.AssetObject as T);
                res.SetResult(op.AssetObject as T);
                if (!this.Temp.ContainsKey(op.AssetObject))
                {
                    this.Temp.Add(op.AssetObject, op);
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
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public ETTask LoadTask<T>(string path,Action<T> callback)where T:UnityEngine.Object
        {
            ETTask task = ETTask.Create(true);
            this.LoadAsync<T>(path, (data) =>
            {
                callback?.Invoke(data);
                task.SetResult();
            }).Coroutine();
            return task;
        }

        /// <summary>
        /// 加载场景
        /// </summary>
        /// <param name="path"></param>
        /// <param name="isAdditive"></param>
        /// <returns></returns>
        public ETTask LoadSceneAsync(string path, bool isAdditive)
        {
            ETTask res = ETTask.Create(true);
            if (string.IsNullOrEmpty(path))
            {
                Log.Error("path err : " + path);
                return res;
            }
            this.loaderCount++;
            var op = YooAssets.LoadSceneAsync(path,isAdditive?LoadSceneMode.Additive:LoadSceneMode.Single);
            op.Completed += (op) =>
            {
                this.loaderCount--;
                res.SetResult();
            };
            return res;
        }


        /// <summary>
        /// 清理资源：切换场景时调用
        /// </summary>
        /// <param name="excludeClearAssets">不需要清除的</param>
        public void ClearAssetsCache(UnityEngine.Object[] excludeClearAssets = null)
        {
            HashSetComponent<AssetOperationHandle> temp = null;
            if (excludeClearAssets != null)
            {
                temp = HashSetComponent<AssetOperationHandle>.Create();
                for (int i = 0; i < excludeClearAssets.Length; i++)
                {
                    temp.Add(this.Temp[excludeClearAssets[i]]);
                }
            }

            for (int i = this.CachedAssetOperationHandles.Count-1; i >=0; i--)
            {
                if (temp == null || !temp.Contains(this.CachedAssetOperationHandles[i]))
                {
                    this.Temp.Remove(this.CachedAssetOperationHandles[i].AssetObject);
                    this.CachedAssetOperationHandles[i].Release();
                    this.CachedAssetOperationHandles.RemoveAt(i);
                }
            }
            YooAssetsMgr.Instance.UnloadUnusedAssets();
        }
        /// <summary>
        /// 释放资源
        /// </summary>
        /// <param name="pooledGo"></param>
        public void ReleaseAsset(UnityEngine.Object pooledGo)
        {
            if (this.Temp.TryGetValue(pooledGo, out var op))
            {
                op.Release();
                this.Temp.Remove(pooledGo);
                this.CachedAssetOperationHandles.Remove(op);
            }
        }

        /// <summary>
        /// 同步加载json或者二进制配置
        /// </summary>
        /// <param name="path"></param>
        /// <param name="type">0:json,1:bytes</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T LoadConfig<T>(string path,int type = 0) where T : class
        {
            if (string.IsNullOrEmpty(path)) return default;
            if (type == 0)
            {
                path += ".json";
                var file = Load<TextAsset>(path);
                try
                {
                    T res = JsonHelper.FromJson<T>(file.text);
                    ReleaseAsset(file);
                    return res;
                }
                catch (Exception ex)
                {
                    Log.Error(ex);
                }
            }
            else if (type == 1)
            {
                path += ".bytes";
                var file = Load<TextAsset>(path);
                try
                {
                    T res = ProtobufHelper.FromBytes<T>(file.bytes);
                    ReleaseAsset(file);
                    return res;
                }
                catch
                {
                }
            }
            Log.Error($"反序列化{TypeInfo<T>.TypeName}失败！{path}");
            return default;
        }
    }
}