using System;
using System.Collections.Generic;
using UnityEngine;

namespace TaoTie
{
    public class MaterialManager:IManager
    {
        public static MaterialManager Instance { get;private set; }
        private Dictionary<string, Material> cacheMaterial;

        #region override

        public void Init()
        {
            Instance = this;
            cacheMaterial = new Dictionary<string, Material>();
        }

        public void Destroy()
        {
            foreach (var item in cacheMaterial)
            {
                ResourcesManager.Instance.ReleaseAsset(item.Value);
            }
            cacheMaterial.Clear();
            Instance = null;
        }

        #endregion

        public Material GetFromCache(string address)
        {
            cacheMaterial.TryGetValue(address, out var res);
            return res;
        }
        public async ETTask PreLoadMaterial(string address)
        {
            Material res;
            CoroutineLock coroutineLock = null;
            try
            {
                coroutineLock = await CoroutineLockManager.Instance.Wait(CoroutineLockType.Resources, address.GetHashCode());
                if (!this.cacheMaterial.TryGetValue(address, out res))
                {
                    res = await ResourcesManager.Instance.LoadAsync<Material>(address);
                    if (res != null)
                        this.cacheMaterial[address] = res;
                }

            }
            finally
            {
                coroutineLock?.Dispose();
            }
        }
        public async ETTask<Material> LoadMaterialAsync(string address, Action<Material> callback = null)
        {
            Material res;
            CoroutineLock coroutineLock = null;
            try
            {
                coroutineLock = await CoroutineLockManager.Instance.Wait(CoroutineLockType.Resources, address.GetHashCode());
                if (!this.cacheMaterial.TryGetValue(address, out res))
                {
                    res = await ResourcesManager.Instance.LoadAsync<Material>(address);
                    if (res != null)
                        this.cacheMaterial[address] = res;
                }
                callback?.Invoke(res);
            }
            finally
            {
                coroutineLock?.Dispose();
            }
            return res;
        }
        
        public ETTask LoadMaterialTask(string address, Action<Material> callback = null)
        {
            ETTask task = ETTask.Create();
            this.LoadMaterialAsync(address, (data) =>
            {
                callback?.Invoke(data);
                task.SetResult();
            }).Coroutine();
            return task;
        }
    }
}