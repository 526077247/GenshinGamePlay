using System;
using System.Collections.Generic;
using UnityEngine;

namespace TaoTie
{
    public class MaterialManager:IManager
    {
        public static MaterialManager Instance { get; set; }
        public Dictionary<string, Material> m_cacheMaterial;

        #region override

        public void Init()
        {
            m_cacheMaterial = new Dictionary<string, Material>();
        }

        public void Destroy()
        {
            foreach (var item in m_cacheMaterial)
            {
                ResourcesManager.Instance.ReleaseAsset(item.Value);
            }
            m_cacheMaterial.Clear();
        }

        #endregion
        
        public async ETTask<Material> LoadMaterialAsync(string address, Action<Material> callback = null)
        {
            Material res;
            CoroutineLock coroutineLock = null;
            try
            {
                coroutineLock = await CoroutineLockManager.Instance.Wait(CoroutineLockType.Resources, address.GetHashCode());
                if (!this.m_cacheMaterial.TryGetValue(address, out res))
                {
                    res = await ResourcesManager.Instance.LoadAsync<Material>(address);
                    if (res != null)
                        this.m_cacheMaterial[address] = res;
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