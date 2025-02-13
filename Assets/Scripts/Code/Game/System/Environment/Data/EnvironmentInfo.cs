using System;
using UnityEngine;

namespace TaoTie
{
    public class EnvironmentInfo: IDisposable
    {
        private bool isDispose;
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
            res.isDispose = false;
            res.SkyCubePath = config.SkyCubePath;
            ResourcesManager.Instance.LoadAsync<Cubemap>(config.SkyCubePath, (cube) =>
            {
                if(res.SkyCube!=null) ResourcesManager.Instance.ReleaseAsset(res.SkyCube);
                if(res.isDispose) ResourcesManager.Instance.ReleaseAsset(cube);
                else res.SkyCube = cube;
            }).Coroutine();
            res.TintColor = config.TintColor;
            return res;
        }

        public static EnvironmentInfo DeepClone(EnvironmentInfo other)
        {
            EnvironmentInfo res = ObjectPool.Instance.Fetch<EnvironmentInfo>();
            res.isDispose = false;
            res.Progress = other.Progress;
            res.SkyCubePath = other.SkyCubePath;
            res.SkyCubePath2 = other.SkyCubePath2;
            ResourcesManager.Instance.LoadAsync<Cubemap>(other.SkyCubePath, (cube) =>
            {
                if(res.SkyCube!=null) ResourcesManager.Instance.ReleaseAsset(res.SkyCube);
                if(res.isDispose) ResourcesManager.Instance.ReleaseAsset(cube);
                else res.SkyCube = cube;
            }).Coroutine();
            
            if (!string.IsNullOrEmpty(res.SkyCubePath2))
            {
                ResourcesManager.Instance.LoadAsync<Cubemap>(other.SkyCubePath2, (cube) =>
                {
                    if(res.SkyCube2!=null) ResourcesManager.Instance.ReleaseAsset(res.SkyCube2);
                    if(res.isDispose) ResourcesManager.Instance.ReleaseAsset(cube);
                    else res.SkyCube2 = cube;
                }).Coroutine();

            }
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
                ResourcesManager.Instance.LoadAsync<Cubemap>(SkyCubePath, (cube) =>
                {
                    if(SkyCube!=null) ResourcesManager.Instance.ReleaseAsset(SkyCube);
                    if(isDispose) ResourcesManager.Instance.ReleaseAsset(cube);
                    else SkyCube = cube;
                }).Coroutine();
            }

            if (before2 != SkyCubePath2)
            {
                ResourcesManager.Instance.LoadAsync<Cubemap>(SkyCubePath2, (cube) =>
                {
                    if(SkyCube2!=null) ResourcesManager.Instance.ReleaseAsset(SkyCube2);
                    if(isDispose) ResourcesManager.Instance.ReleaseAsset(cube);
                    else SkyCube2 = cube;
                }).Coroutine();
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
                ResourcesManager.Instance.LoadAsync<Cubemap>(SkyCubePath, (cube) =>
                {
                    if(SkyCube!=null) ResourcesManager.Instance.ReleaseAsset(SkyCube);
                    if(isDispose) ResourcesManager.Instance.ReleaseAsset(cube);
                    else SkyCube = cube;
                }).Coroutine();
            }

            if (before2 != to.SkyCubePath)
            {
                SkyCubePath2 = to.SkyCubePath;
                ResourcesManager.Instance.LoadAsync<Cubemap>(SkyCubePath2, (cube) =>
                {
                    if(SkyCube2!=null) ResourcesManager.Instance.ReleaseAsset(SkyCube2);
                    if(isDispose) ResourcesManager.Instance.ReleaseAsset(cube);
                    else SkyCube2 = cube;
                }).Coroutine();
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
            this.isDispose = true;
            ObjectPool.Instance.Recycle(this);
        }
    }
}