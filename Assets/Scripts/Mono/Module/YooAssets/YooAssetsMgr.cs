using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TaoTie;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace YooAsset
{
    public class YooAssetsMgr
    {
        public static YooAssetsMgr Instance { get; private set; } = new YooAssetsMgr();

        public CDNConfig CdnConfig;
        public BuildInConfig Config;
        public ResourcePackage DefaultPackage;
        public EPlayMode PlayMode;
        public const string DefaultName = "DefaultPackage";

        private readonly Dictionary<string, ResourcePackage> packages = new Dictionary<string, ResourcePackage>();
        private HostPlayModeParameters hostPlayModeParameters;
        private OfflinePlayModeParameters offlinePlayModeParameters;
        public async ETTask Init(EPlayMode mode)
        {
            PlayMode = mode;
            // 初始化资源系统
            YooAssets.Initialize();
            // 创建默认的资源包
            var package = await GetPackage(DefaultName);
            DefaultPackage = package;
            // 设置该资源包为默认的资源包，可以使用YooAssets相关加载接口加载该资源包内容。
            YooAssets.SetDefaultPackage(package);
            await UpdateConfig();
        }

        private async ETTask InitPackage(EPlayMode mode,ResourcePackage package, string packageName)
        {
#if UNITY_EDITOR
            // 编辑器下的模拟模式
            if (mode == EPlayMode.EditorSimulateMode)
            {
                if (CdnConfig == null)
                {
                    CdnConfig = Resources.Load<CDNConfig>("CDNConfig");
                }

                var initParameters = new EditorSimulateModeParameters();
                initParameters.SimulateManifestFilePath = EditorSimulateModeHelper.SimulateBuild(packageName);
                var op = package.InitializeAsync(initParameters);
                await op.Task;
                if (op.Status == EOperationStatus.Failed)
                {
                    Log.Error(op.Error);
                }
            }
            else
#endif
#if UNITY_WEBGL
            if (mode == EPlayMode.WebPlayMode)
            {
                if (CdnConfig == null)
                {
                    CdnConfig = Resources.Load<CDNConfig>("CDNConfig");
                }
                var initParameters = new WebPlayModeParameters();
                initParameters.RemoteServices = new RemoteServices(CdnConfig);
                initParameters.BuildinQueryServices = new GameQueryServices();
                initParameters.BuildinRootDirectory = StreamingAssetsDefine.StreamAssetsDir;
            }
            else
#endif
                // 单机运行模式
            if (mode == EPlayMode.OfflinePlayMode)
            {
                if (offlinePlayModeParameters == null)
                {
                    var initParameters = new OfflinePlayModeParameters();
#if !UNITY_WEBGL
                    initParameters.DecryptionServices = new BundleDecryption();
#endif
                    initParameters.BuildinRootDirectory = StreamingAssetsDefine.StreamAssetsDir;
                    offlinePlayModeParameters = initParameters;
                }

                var op = package.InitializeAsync(offlinePlayModeParameters);
                await op.Task;
                if (op.Status == EOperationStatus.Failed)
                {
                    Log.Error(op.Error);
                }
            }
            // 联机运行模式
            else
            {
                if (CdnConfig == null)
                {
                    CdnConfig = Resources.Load<CDNConfig>("CDNConfig");
                }

                if (hostPlayModeParameters == null)
                {
                    var initParameters = new HostPlayModeParameters();
                    initParameters.RemoteServices = new RemoteServices(CdnConfig);
#if !UNITY_WEBGL
                    initParameters.DecryptionServices = new BundleDecryption();
#endif
                    initParameters.BuildinQueryServices = new GameQueryServices();
                    initParameters.DeliveryQueryServices = new DefaultDeliveryQueryServices();
                    initParameters.BuildinRootDirectory = StreamingAssetsDefine.StreamAssetsDir;
                    hostPlayModeParameters = initParameters;
                }
                var op = package.InitializeAsync(hostPlayModeParameters);
                await op.Task;
                if (op.Status == EOperationStatus.Failed)
                {
                    Log.Error(op.Error);
                }
            }
        }

        public async ETTask<ResourcePackage> GetPackage(string package)
        {
            if (package == null) package = DefaultName;
            if (packages.TryGetValue(package, out var res))
            {
                return res;
            }
            res = YooAssets.CreatePackage(package);
            packages.Add(package,res);
            await InitPackage(PlayMode, res, package);
            return res;
        }
        
        /// <summary>
        /// 注意只能是已经load过的
        /// </summary>
        /// <param name="package"></param>
        /// <returns></returns>
        public ResourcePackage GetPackageSync(string package)
        {
            if (package == null) package = DefaultName;
            if (packages.TryGetValue(package, out var res))
            {
                return res;
            }
            Log.Error("GetPackageSync fail package =" + package);
            GetPackage(package).Coroutine();
            return null;
        }

        public async ETTask UpdateConfig()
        {
            var op = DefaultPackage.LoadRawFileSync("config.bytes");
            if (op == null)return;
            await op.Task;
            var conf = op.GetRawFileText();
            Config = JsonHelper.FromJson<BuildInConfig>(conf);
            if (Config == null)
            {
                Log.Error("UpdateConfig Config == null");
            }
            op.Release();
        }
        public void UnloadUnusedAssets()
        {
            UnloadUnusedAssets(DefaultName);
        }
        public void UnloadUnusedAssets(string package)
        {
            var packageInfo = GetPackageSync(package);
            packageInfo?.UnloadUnusedAssets();
            Log.Info("UnloadUnusedAssets "+package);
        }
        
        public void ForceUnloadAllAssets()
        {
            ForceUnloadAllAssets(DefaultName);
        }
        public void ForceUnloadAllAssets(string package)
        {
            var packageInfo = GetPackageSync(package);
            packageInfo?.ForceUnloadAllAssets();
        }

        public AssetOperationHandle LoadAssetSync<T>(string path,string package) where T : UnityEngine.Object
        {
            var packageInfo = GetPackageSync(package);
            if (packageInfo == null) return null;
            return packageInfo.LoadAssetSync<T>(path);
        }
        public AssetOperationHandle LoadAssetSync(AssetInfo assetInfo,string package)
        {
            var packageInfo = GetPackageSync(package);
            if (packageInfo == null) return null;
            return packageInfo.LoadAssetSync(assetInfo);
        }
        public AssetOperationHandle LoadAssetAsync(AssetInfo assetInfo, string package)
        {
            var packageInfo = GetPackageSync(package);
            if (packageInfo == null) return null;
            return packageInfo.LoadAssetAsync(assetInfo);
        }
        public AssetOperationHandle LoadAssetAsync<T>(string path,string package) where T : UnityEngine.Object
        {
            var packageInfo = GetPackageSync(package);
            if (packageInfo == null) return null;
            return packageInfo.LoadAssetAsync<T>(path);
        }
        
        public SceneOperationHandle LoadSceneAsync(string path,LoadSceneMode mode,string package)
        {
            var packageInfo = GetPackageSync(package);
            if (packageInfo == null) return null;
            return packageInfo.LoadSceneAsync(path,mode);
        }

        public ResourceDownloaderOperation CreateResourceDownloader(int downloadingMaxNumber, int failedTryAgain, int timeout,string package,string[] tags = null)
        {
            var packageInfo = GetPackageSync(package);
            if (packageInfo == null) return null;
            if (tags == null)
                return packageInfo.CreateResourceDownloader(downloadingMaxNumber, failedTryAgain, timeout);
            return packageInfo.CreateResourceDownloader(tags, downloadingMaxNumber, failedTryAgain, timeout);
        }
        public UpdatePackageManifestOperation UpdatePackageManifestAsync(string packageVersion, bool autoSaveVersion , int timeout ,string package)
        {
            var packageInfo = GetPackageSync(package);
            if (packageInfo == null) return null;
            return packageInfo.UpdatePackageManifestAsync(packageVersion,autoSaveVersion,timeout);
        }

        public AssetInfo[] GetAssetInfos(string tag, string package)
        {
            var packageInfo = GetPackageSync(package);
            if (packageInfo == null) return Array.Empty<AssetInfo>();
            return packageInfo.GetAssetInfos(tag);
        }

        public bool IsNeedDownloadFromRemote(string path, string package)
        {
            var packageInfo = GetPackageSync(package);
            if (packageInfo == null) return false;
            return packageInfo.IsNeedDownloadFromRemote(path);
        }
    }
}