using System;
using UnityEngine;

namespace TaoTie
{
    public class EffectInfo: IDisposable
    {
        public static EffectInfo Create()
        {
            var res = ObjectPool.Instance.Fetch<EffectInfo>();
            res.IsDispose = false;
            return res;
        }

        private bool IsDispose;
        public GameObject obj;
        public int ConfigId;
        public GameObjectHolderComponent Parent;
        public long TimerId;
        public void Dispose()
        {
            if (IsDispose)
            {
                return;
            }

            IsDispose = true;
            GameTimerManager.Instance.Remove(ref TimerId);
            Parent = default;
            ConfigId = default;
            GameObjectPoolManager.Instance.RecycleGameObject(obj);
            obj = null;
            ObjectPool.Instance.Recycle(this);
        }
    }
}