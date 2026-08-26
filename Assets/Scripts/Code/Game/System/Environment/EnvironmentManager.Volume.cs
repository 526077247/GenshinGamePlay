using UnityEngine.Rendering;
using UnityEngine;

namespace TaoTie
{
    public partial class EnvironmentManager
    {
        private Volume volumeA;
        private Volume volumeB;
        private bool volumeIsBlending;
        private string activeVolumePath;

        private partial void ApplyVolume(EnvironmentInfo info)
        {
            if (info == null)
            {
                if (volumeA != null) volumeA.enabled = false;
                if (volumeB != null) volumeB.enabled = false;
                activeVolumePath = null;
                volumeIsBlending = false;
                return;
            }

            if (!info.IsBlender)
            {
                SetVolumeProfile(info.VolumeProfilePath);
                volumeIsBlending = false;
                return;
            }

            // 混合状态：双 Volume 交叉淡入淡出
            if (!volumeIsBlending)
            {
                EnsureVolume(ref volumeA, "EnvironmentVolume_A");
                EnsureVolume(ref volumeB, "EnvironmentVolume_B");
                LoadProfileAsync(info.VolumeProfilePath, (profile) =>
                {
                    if (volumeA != null) volumeA.profile = profile;
                });
                LoadProfileAsync(info.VolumeProfilePath2, (profile) =>
                {
                    if (volumeB != null) volumeB.profile = profile;
                });
                volumeA.weight = 1f;
                volumeB.weight = 0f;
                volumeIsBlending = true;
            }

            volumeA.weight = 1f - info.Progress;
            volumeB.weight = info.Progress;
        }

        private void SetVolumeProfile(string path)
        {
            if (path == activeVolumePath) return;
            EnsureVolume(ref volumeA, "EnvironmentVolume_A");

            if (string.IsNullOrEmpty(path))
            {
                volumeA.enabled = false;
                activeVolumePath = null;
                return;
            }

            LoadProfileAsync(path, (profile) =>
            {
                if (volumeA != null)
                {
                    volumeA.profile = profile;
                    volumeA.weight = 1f;
                    volumeA.enabled = true;
                }
            });
            if (volumeB != null) volumeB.enabled = false;
            activeVolumePath = path;
        }

        private void EnsureVolume(ref Volume vol, string name)
        {
            if (vol != null) return;
            var go = new GameObject(name);
            vol = go.AddComponent<Volume>();
            vol.isGlobal = true;
            vol.priority = 100;
            vol.enabled = false;
            GameObject.DontDestroyOnLoad(go);
        }

        private void LoadProfileAsync(string path, System.Action<VolumeProfile> callback)
        {
            if (string.IsNullOrEmpty(path))
            {
                callback?.Invoke(null);
                return;
            }
            ResourcesManager.Instance.LoadAsync<VolumeProfile>(path, (profile) =>
            {
                callback?.Invoke(profile);
            }).Coroutine();
        }

        private void DestroyVolumes()
        {
            if (volumeA != null)
            {
                UnityEngine.Object.Destroy(volumeA.gameObject);
                volumeA = null;
            }
            if (volumeB != null)
            {
                UnityEngine.Object.Destroy(volumeB.gameObject);
                volumeB = null;
            }
            activeVolumePath = null;
            volumeIsBlending = false;
        }
    }
}
