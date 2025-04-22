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
        
        public Color LightColor;
        public float LightIntensity;
        public Vector3 LightDir;
        public bool UseDirLight;
        public LightShadows LightShadows;

        public static EnvironmentInfo Create(ConfigEnvironment config)
        {
            EnvironmentInfo res = ObjectPool.Instance.Fetch<EnvironmentInfo>();
            res.isDispose = false;
            res.SkyCubePath = config.SkyCubePath;
            if (!string.IsNullOrEmpty(config.SkyCubePath))
            {
                ResourcesManager.Instance.LoadAsync<Cubemap>(config.SkyCubePath, (cube) =>
                {
                    if (res.SkyCube != null) ResourcesManager.Instance.ReleaseAsset(res.SkyCube);
                    if (res.isDispose) ResourcesManager.Instance.ReleaseAsset(cube);
                    else res.SkyCube = cube;
                }).Coroutine();
            }

            res.UseDirLight = config.UseDirLight;
            res.TintColor = config.TintColor;
            res.LightColor = config.LightColor;
            res.LightIntensity = config.LightIntensity;
            res.LightDir = config.LightDir;
            res.LightShadows = config.LightShadows;
            return res;
        }

        public static EnvironmentInfo DeepClone(EnvironmentInfo other)
        {
            EnvironmentInfo res = ObjectPool.Instance.Fetch<EnvironmentInfo>();
            res.isDispose = false;
            res.Progress = other.Progress;
            res.SkyCubePath = other.SkyCubePath;
            res.SkyCubePath2 = other.SkyCubePath2;
            if (!string.IsNullOrEmpty(res.SkyCubePath))
            {
                ResourcesManager.Instance.LoadAsync<Cubemap>(res.SkyCubePath, (cube) =>
                {
                    if (res.SkyCube != null) ResourcesManager.Instance.ReleaseAsset(res.SkyCube);
                    if (res.isDispose) ResourcesManager.Instance.ReleaseAsset(cube);
                    else res.SkyCube = cube;
                }).Coroutine();
            }
            else
            {
                if (res.SkyCube != null) ResourcesManager.Instance.ReleaseAsset(res.SkyCube);
                res.SkyCube = null;
            }

            if (!string.IsNullOrEmpty(res.SkyCubePath2))
            {
                ResourcesManager.Instance.LoadAsync<Cubemap>(res.SkyCubePath2, (cube) =>
                {
                    if (res.SkyCube2 != null) ResourcesManager.Instance.ReleaseAsset(res.SkyCube2);
                    if(res.isDispose) ResourcesManager.Instance.ReleaseAsset(cube);
                    else res.SkyCube2 = cube;
                }).Coroutine();
            }
            else
            {
                if (res.SkyCube2 != null) ResourcesManager.Instance.ReleaseAsset(res.SkyCube2);
                res.SkyCube2 = null;
            }
            res.TintColor = other.TintColor;
            res.TintColor2 = other.TintColor2;
            res.LightColor = other.LightColor;
            res.LightIntensity = other.LightIntensity;
            res.LightDir = other.LightDir;
            res.UseDirLight = other.UseDirLight;
            res.LightShadows = other.LightShadows;
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

            if (before != from.SkyCubePath)
            {
                SkyCubePath = from.SkyCubePath;
                if (!string.IsNullOrEmpty(SkyCubePath))
                {
                    ResourcesManager.Instance.LoadAsync<Cubemap>(SkyCubePath, (cube) =>
                    {
                        if (SkyCube != null) ResourcesManager.Instance.ReleaseAsset(SkyCube);
                        if (isDispose) ResourcesManager.Instance.ReleaseAsset(cube);
                        else SkyCube = cube;
                    }).Coroutine();
                }
                else
                {
                    if (SkyCube != null) ResourcesManager.Instance.ReleaseAsset(SkyCube);
                    SkyCube = null;
                }
            }

            if (before2 != to.SkyCubePath)
            {
                SkyCubePath2 = to.SkyCubePath;
                if (!string.IsNullOrEmpty(SkyCubePath2))
                {
                    ResourcesManager.Instance.LoadAsync<Cubemap>(SkyCubePath2, (cube) =>
                    {
                        if (SkyCube2 != null) ResourcesManager.Instance.ReleaseAsset(SkyCube2);
                        if (isDispose) ResourcesManager.Instance.ReleaseAsset(cube);
                        else SkyCube2 = cube;
                    }).Coroutine();
                }
                else
                {
                    if (SkyCube2 != null) ResourcesManager.Instance.ReleaseAsset(SkyCube2);
                    SkyCube2 = null;
                }
            }

            TintColor = from.TintColor;
            TintColor2 = to.TintColor;
            UseDirLight = from.UseDirLight || to.UseDirLight;
            if (from.UseDirLight && to.UseDirLight)
            {
                LightColor = Color.Lerp(from.LightColor,to.LightColor,val);
                LightIntensity = Mathf.Lerp(from.LightIntensity,to.LightIntensity,val);
                LightDir = Vector3.Lerp(from.LightDir,to.LightDir,val);
                LightShadows = to.LightShadows;
            }
            else if (from.UseDirLight)
            {
                LightColor = from.LightColor;
                LightIntensity = from.LightIntensity;
                LightDir = from.LightDir;
                LightShadows = from.LightShadows;
            }
            else if (to.UseDirLight)
            {
                LightColor = to.LightColor;
                LightIntensity = to.LightIntensity;
                LightDir = to.LightDir;
                LightShadows = to.LightShadows;
            }
        }
        public void Lerp(ConfigEnvironment from, ConfigEnvironment to, float val)
        {
            Progress = val;
            var before = SkyCubePath;
            var before2 = SkyCubePath2;
            if (before != from.SkyCubePath)
            {
                SkyCubePath = from.SkyCubePath;
                if (!string.IsNullOrEmpty(SkyCubePath))
                {
                    ResourcesManager.Instance.LoadAsync<Cubemap>(SkyCubePath, (cube) =>
                    {
                        if (SkyCube != null) ResourcesManager.Instance.ReleaseAsset(SkyCube);
                        if (isDispose) ResourcesManager.Instance.ReleaseAsset(cube);
                        else SkyCube = cube;
                    }).Coroutine();
                }
                else
                {
                    if (SkyCube != null) ResourcesManager.Instance.ReleaseAsset(SkyCube);
                    SkyCube = null;
                }
            }

            if (before2 != to.SkyCubePath)
            {
                SkyCubePath2 = to.SkyCubePath;
                if (!string.IsNullOrEmpty(SkyCubePath2))
                {
                    ResourcesManager.Instance.LoadAsync<Cubemap>(SkyCubePath2, (cube) =>
                    {
                        if (SkyCube2 != null) ResourcesManager.Instance.ReleaseAsset(SkyCube2);
                        if (isDispose) ResourcesManager.Instance.ReleaseAsset(cube);
                        else SkyCube2 = cube;
                    }).Coroutine();
                }
                else
                {
                    if (SkyCube2 != null) ResourcesManager.Instance.ReleaseAsset(SkyCube2);
                    SkyCube2 = null;
                }
            }
            TintColor = from.TintColor;
            TintColor2 = to.TintColor;
            UseDirLight = from.UseDirLight || to.UseDirLight;
            if (from.UseDirLight && to.UseDirLight)
            {
                LightColor = Color.Lerp(from.LightColor,to.LightColor,val);
                LightIntensity = Mathf.Lerp(from.LightIntensity,to.LightIntensity,val);
                LightDir = Vector3.Lerp(from.LightDir,to.LightDir,val);
                LightShadows = to.LightShadows;
            }
            else if (from.UseDirLight)
            {
                LightColor = from.LightColor;
                LightIntensity = from.LightIntensity;
                LightDir = from.LightDir;
                LightShadows = from.LightShadows;
            }
            else if (to.UseDirLight)
            {
                LightColor = to.LightColor;
                LightIntensity = to.LightIntensity;
                LightDir = to.LightDir;
                LightShadows = to.LightShadows;
            }
        }
        public void Dispose()
        {
            Progress = default;
            if (SkyCube != null)
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
            LightColor = default;
            LightIntensity = default;
            LightDir = default;
            UseDirLight = default;
            this.isDispose = true;
            ObjectPool.Instance.Recycle(this);
        }
    }
}