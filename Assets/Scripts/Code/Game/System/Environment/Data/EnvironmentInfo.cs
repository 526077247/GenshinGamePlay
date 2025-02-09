using System;
using UnityEngine;

namespace TaoTie
{
    public class EnvironmentInfo: IDisposable
    {
        public bool Changed;
        public bool IsBlender;

        public float Progress;
        public string SkyCubePath;
        public string SkyCubePath2;
        
        public Cubemap SkyCube;
        public Cubemap SkyCube2;
        public Color TintColor;
        public Color TintColor2;
        
        public static EnvironmentInfo Create(ConfigEnvironment config)
        {
            EnvironmentInfo res = ObjectPool.Instance.Fetch<EnvironmentInfo>();
            res.SkyCubePath = config.SkyCubePath;
            res.SkyCube = ResourcesManager.Instance.Load<Cubemap>(config.SkyCubePath);
            res.TintColor = config.TintColor;
            return res;
        }

        public static EnvironmentInfo DeepClone(EnvironmentInfo other)
        {
            EnvironmentInfo res = ObjectPool.Instance.Fetch<EnvironmentInfo>();
            res.Progress = other.Progress;
            res.SkyCubePath = other.SkyCubePath;
            res.SkyCubePath2 = other.SkyCubePath2;
            res.SkyCube = ResourcesManager.Instance.Load<Cubemap>(other.SkyCubePath);
            if(!string.IsNullOrEmpty(res.SkyCubePath2)) 
                res.SkyCube2 = ResourcesManager.Instance.Load<Cubemap>(other.SkyCubePath2);
            res.TintColor = other.TintColor;
            res.TintColor2 = other.TintColor2;
            return res;
        }

        public void Lerp(EnvironmentInfo from, EnvironmentInfo to, float val)
        {
            Progress = val;
            var before = SkyCubePath;
            var before2 = SkyCubePath2;
            if (from.IsBlender)
                SkyCubePath = from.Progress > 0.5 ? from.SkyCubePath2 : from.SkyCubePath;
            else
                SkyCubePath = from.SkyCubePath;

            if (to.IsBlender)
                SkyCubePath2 = to.Progress > 0.5 ? to.SkyCubePath2 : to.SkyCubePath;
            else
                SkyCubePath2 = to.SkyCubePath;

            if (before != SkyCubePath)
            {
                if(SkyCube!=null) ResourcesManager.Instance.ReleaseAsset(SkyCube);
                SkyCube = ResourcesManager.Instance.Load<Cubemap>(SkyCubePath);
            }

            if (before2 != SkyCubePath2)
            {
                if(SkyCube2!=null) ResourcesManager.Instance.ReleaseAsset(SkyCube2);
                SkyCube2 = ResourcesManager.Instance.Load<Cubemap>(SkyCubePath2);
            }

            TintColor = from.TintColor;
            TintColor2 = to.TintColor;
        }
        public void Lerp(ConfigEnvironment from, ConfigEnvironment to, float val)
        {
            Progress = val;
            var before = SkyCubePath;
            var before2 = SkyCubePath2;
            if (before != from.SkyCubePath)
            {
                SkyCubePath = from.SkyCubePath;
                if(SkyCube!=null) ResourcesManager.Instance.ReleaseAsset(SkyCube);
                SkyCube = ResourcesManager.Instance.Load<Cubemap>(SkyCubePath);
            }

            if (before2 != to.SkyCubePath)
            {
                SkyCubePath2 = to.SkyCubePath;
                if(SkyCube2!=null) ResourcesManager.Instance.ReleaseAsset(SkyCube2);
                SkyCube2 = ResourcesManager.Instance.Load<Cubemap>(SkyCubePath2);
            }
            TintColor = from.TintColor;
            TintColor2 = to.TintColor;
        }
        public void Dispose()
        {
            Progress = default;
            if (SkyCube2 != null)
            {
                ResourcesManager.Instance.ReleaseAsset(SkyCube);
                SkyCube = null;
            }

            if (SkyCube2 != null)
            {
                ResourcesManager.Instance.ReleaseAsset(SkyCube2);
                SkyCube2 = null;
            }

            TintColor = default;
            TintColor2 = default;
            ObjectPool.Instance.Recycle(this);
        }
    }
}