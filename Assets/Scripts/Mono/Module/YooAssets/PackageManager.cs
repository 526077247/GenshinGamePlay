using System;
using System.Collections.Generic;
using Obfuz;
using Obfuz.EncryptionVM;
using YooAsset;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TaoTie
{
    public class PackageManager
    {
        public static PackageManager Instance { get; } = new PackageManager();

        public CDNConfig CdnConfig;
        public PackageConfig Config;
        public ResourcePackage DefaultPackage;
        public BuildInPackageConfig BuildInPackageConfig;
        public EPlayMode PlayMode;

        private readonly Dictionary<string, ResourcePackage> packages = new Dictionary<string, ResourcePackage>();
        private InitializeParameters initializeParameters;

        public async ETTask Init(EPlayMode mode)
        {
            InitBuildInPackageVersion();
            PlayMode = mode;
            // 初始化资源系统
            YooAssets.SetOperationSystemQuickStartMode(true);
            YooAssets.Initialize();
            // 创建默认的资源包
            var package = await GetPackageAsync(Define.DefaultName);
            DefaultPackage = package;
            // 设置该资源包为默认的资源包，可以使用YooAssets相关加载接口加载该资源包内容。
            YooAssets.SetDefaultPackage(package);
            await UpdateConfig();
            await SetUpDynamicSecret();
        }

        private void InitBuildInPackageVersion()
        {
            BuildInPackageConfig = Resources.Load<BuildInPackageConfig>("BuildInPackageConfig");
#if !UNITY_EDITOR
            if (BuildInPackageConfig == null) return;
            for (int i = 0; i < BuildInPackageConfig.PackageName.Count; i++)
            {
                var name = BuildInPackageConfig.PackageName[i];
                var ver = GetPackageVersion();
                if (ver < 0 || ver < BuildInPackageConfig.PackageVer[i])
                {
                    PlayerPrefs.SetInt("PACKAGE_VERSION_" + name, BuildInPackageConfig.PackageVer[i]);
                }
            }
            PlayerPrefs.Save();
#endif
        }

        private async ETTask InitPackage(EPlayMode mode, ResourcePackage package)
        {
            string packageName = package.PackageName;
#if UNITY_EDITOR
            // 编辑器下的模拟模式
            if (mode == EPlayMode.EditorSimulateMode)
            {
                if (initializeParameters == null)
                {
                    if (CdnConfig == null)
                    {
                        CdnConfig = Resources.Load<CDNConfig>("CDNConfig");
                    }

                    var buildResult = EditorSimulateModeHelper.SimulateBuild(packageName);
                    var packageRoot = buildResult.PackageRootDirectory;
                    var editorFileSystemParams =
                        FileSystemParameters.CreateDefaultEditorFileSystemParameters(packageRoot);
                    var initParameters = new EditorSimulateModeParameters();
                    initParameters.EditorFileSystemParameters = editorFileSystemParams;
                    initializeParameters = initParameters;
                }

                var op = package.InitializeAsync(initializeParameters);
                await op.Task;
                if (op.Status == EOperationStatus.Failed)
                {
                    Log.Error(op.Error);
                }
            }
            else
#endif
#if UNITY_WEBGL
            {
                if (initializeParameters == null)
                {
                    if (CdnConfig == null)
                    {
                        CdnConfig = Resources.Load<CDNConfig>("CDNConfig");
                    }

                    IRemoteServices remoteServices = new RemoteServices(CdnConfig);
                    var webServerFileSystemParams =
                        FileSystemParameters.CreateDefaultWebServerFileSystemParameters(new WebDecryption());
                    var webRemoteFileSystemParams =
                        FileSystemParameters.CreateDefaultWebRemoteFileSystemParameters(remoteServices,
                            new WebDecryption()); //支持跨域下载

                    var initParameters = new WebPlayModeParameters();
                    initParameters.WebServerFileSystemParameters = webServerFileSystemParams;
                    initParameters.WebRemoteFileSystemParameters = webRemoteFileSystemParams;
                    initializeParameters = initParameters;
                }
                
                var op = package.InitializeAsync(initializeParameters);
                await op.Task;
                if (op.Status == EOperationStatus.Failed)
                {
                    Log.Error(op.Error);
                }
            }
#else
            // 单机运行模式
            if (mode == EPlayMode.OfflinePlayMode)
            {
                if (initializeParameters == null)
                {
                    var buildinFileSystemParams =
                        FileSystemParameters.CreateDefaultBuildinFileSystemParameters(new FileStreamDecryption());
                    if (BuildInPackageConfig != null && BuildInPackageConfig.PackageName.Contains(packageName))
                        buildinFileSystemParams.AddParameter(FileSystemParametersDefine.COPY_BUILDIN_PACKAGE_MANIFEST,
                            true);
                    var initParameters = new OfflinePlayModeParameters();
                    initParameters.BuildinFileSystemParameters = buildinFileSystemParams;
                    initializeParameters = initParameters;
                }

                var op = package.InitializeAsync(initializeParameters);
                await op.Task;
                if (op.Status == EOperationStatus.Failed)
                {
                    Log.Error(op.Error);
                }
            }
            // 联机运行模式
            else
            {
                if (initializeParameters == null)
                {
                    if (CdnConfig == null)
                    {
                        CdnConfig = Resources.Load<CDNConfig>("CDNConfig");
                    }

                    IRemoteServices remoteServices = new RemoteServices(CdnConfig);
                    var cacheFileSystemParams =
                        FileSystemParameters.CreateDefaultCacheFileSystemParameters(remoteServices,
                            new FileStreamDecryption());
                    var buildinFileSystemParams =
                        FileSystemParameters.CreateDefaultBuildinFileSystemParameters(new FileStreamDecryption());
                    if (BuildInPackageConfig != null && BuildInPackageConfig.PackageName.Contains(packageName))
                        buildinFileSystemParams.AddParameter(FileSystemParametersDefine.COPY_BUILDIN_PACKAGE_MANIFEST,
                            true);
                    var initParameters = new HostPlayModeParameters();
                    initParameters.BuildinFileSystemParameters = buildinFileSystemParams;
                    initParameters.CacheFileSystemParameters = cacheFileSystemParams;
                    initializeParameters = initParameters;
                }

                var op = package.InitializeAsync(initializeParameters);
                await op.Task;
                if (op.Status == EOperationStatus.Failed)
                {
                    Log.Error(op.Error);
                }
            }
#endif
            string version;
            if (mode == EPlayMode.EditorSimulateMode)
            {
                version = "Simulate";
            }
            else
            {
                version = GetPackageVersion(packageName).ToString();
            }
            var manifestOp = package.UpdatePackageManifestAsync(version);
            await manifestOp.Task;
            if (manifestOp.Status != EOperationStatus.Succeed)
            {
                Log.Error("加载本地资源清单文件失败！\r\n" + manifestOp.Error);
            }
        }

        /// <summary>
        /// 异步获取包，必要时初始化
        /// </summary>
        /// <param name="package"></param>
        /// <returns></returns>
        public async ETTask<ResourcePackage> GetPackageAsync(string package)
        {
            if (package == null) package = Define.DefaultName;
            if (packages.TryGetValue(package, out var res))
            {
                return res;
            }

            res = YooAssets.CreatePackage(package);
            packages.Add(package, res);
            await InitPackage(PlayMode, res);
            return res;
        }

        /// <summary>
        /// 同步获取包，注意只能是已经初始化过的
        /// </summary>
        /// <param name="package"></param>
        /// <returns></returns>
        public ResourcePackage GetPackageSync(string package)
        {
            if (package == null) package = Define.DefaultName;
            if (packages.TryGetValue(package, out var res))
            {
                return res;
            }

            Log.Error("GetPackageSync fail package =" + package);
            GetPackageAsync(package).Coroutine();
            return null;
        }

        public async ETTask UpdateConfig()
        {
            var op = DefaultPackage.LoadAssetAsync("config.bytes");
            await op.Task;
            var conf = op.GetAssetObject<TextAsset>().text;
            Config = JsonHelper.FromJson<PackageConfig>(conf);
            if (Config == null)
            {
                Log.Error("UpdateConfig Config == null");
            }

            op.Release();
        }
        
        private async ETTask SetUpDynamicSecret()
        {
            Log.Info("SetUpDynamicSecret begin");
            byte[] dynamicSecretKey;
            #if UNITY_EDITOR
            await ETTask.CompletedTask;
            dynamicSecretKey = UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/AssetsPackage/Code/Obfuz/defaultDynamicSecretKey.bytes").bytes;
            #else
            var op = LoadAssetAsync<TextAsset>($"Code/Obfuz/defaultDynamicSecretKey.bytes", Define.DefaultName);
            await op.Task;
            dynamicSecretKey = (op.AssetObject as TextAsset)?.bytes;
            op.Release();
            #endif
            EncryptionService<DefaultDynamicEncryptionScope>.Encryptor = new GeneratedEncryptionVirtualMachine(dynamicSecretKey);
            Log.Info("SetUpDynamicSecret end");
        }

        public async ETTask UnloadUnusedAssets(string package)
        {
            if (package == null) package = Define.DefaultName;
            if (!packages.TryGetValue(package, out var packageInfo))
            {
                return;
            }

            var task = packageInfo.UnloadUnusedAssetsAsync();
            await task.Task;
            if (task.Status != EOperationStatus.Succeed)
            {
                Log.Error("UnloadUnusedAssets fail \r\n" + task.Error);
            }
        }

        public async ETTask ForceUnloadAllAssets(string package)
        {
            if (package == null) package = Define.DefaultName;
            if (!packages.TryGetValue(package, out var packageInfo))
            {
                return;
            }

            var task = packageInfo.UnloadAllAssetsAsync();
            await task.Task;
            if (task.Status != EOperationStatus.Succeed)
            {
                Log.Error("ForceUnloadAllAssets fail \r\n" + task.Error);
            }
        }

        public AssetHandle LoadAssetSync<T>(string path, string package) where T : UnityEngine.Object
        {
            var packageInfo = GetPackageSync(package);
            if (packageInfo == null) return null;
            return packageInfo.LoadAssetSync<T>(path);
        }

        public AssetHandle LoadAssetSync(AssetInfo assetInfo, string package)
        {
            var packageInfo = GetPackageSync(package);
            if (packageInfo == null) return null;
            return packageInfo.LoadAssetSync(assetInfo);
        }

        public AssetHandle LoadAssetAsync(AssetInfo assetInfo, string package)
        {
            var packageInfo = GetPackageSync(package);
            if (packageInfo == null) return null;
            return packageInfo.LoadAssetAsync(assetInfo);
        }

        public AssetHandle LoadAssetAsync<T>(string path, string package) where T : UnityEngine.Object
        {
            var packageInfo = GetPackageSync(package);
            if (packageInfo == null) return null;
            return packageInfo.LoadAssetAsync<T>(path);
        }

        public SceneHandle LoadSceneAsync(string path, LoadSceneMode mode, string package)
        {
            var packageInfo = GetPackageSync(package);
            if (packageInfo == null) return null;
            return packageInfo.LoadSceneAsync(path, mode);
        }

        public ResourceDownloaderOperation CreateResourceDownloader(int downloadingMaxNumber, int failedTryAgain,
            int timeout, string package, string[] tags = null)
        {
            var packageInfo = GetPackageSync(package);
            if (packageInfo == null) return null;
            if (tags == null)
                return packageInfo.CreateResourceDownloader(downloadingMaxNumber, failedTryAgain, timeout);
            return packageInfo.CreateResourceDownloader(tags, downloadingMaxNumber, failedTryAgain, timeout);
        }

        public UpdatePackageManifestOperation UpdatePackageManifestAsync(string packageVersion, int timeout,
            string package)
        {
            var packageInfo = GetPackageSync(package);
            if (packageInfo == null) return null;
            return packageInfo.UpdatePackageManifestAsync(packageVersion, timeout);
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

        public int GetPackageVersion(string package = Define.DefaultName)
        {
            return UnityEngine.PlayerPrefs.GetInt("PACKAGE_VERSION_" + package, -1);
        }
    }
}