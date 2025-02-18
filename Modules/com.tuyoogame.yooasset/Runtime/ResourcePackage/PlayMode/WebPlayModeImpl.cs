using System;
using System.Collections;
using System.Collections.Generic;

namespace YooAsset
{
    internal class WebPlayModeImpl : IPlayMode, IBundleQuery
    {
        public readonly string PackageName;
        public IFileSystem WebServerFileSystem { set; get; } //可以为空！
        public IFileSystem WebRemoteFileSystem { set; get; } //可以为空！


        public WebPlayModeImpl(string packageName)
        {
            PackageName = packageName;
        }

        /// <summary>
        /// 异步初始化
        /// </summary>
        public InitializationOperation InitializeAsync(WebPlayModeParameters initParameters)
        {
            var operation = new WebPlayModeInitializationOperation(this, initParameters);
            OperationSystem.StartOperation(PackageName, operation);
            return operation;
        }

        #region IPlayMode接口
        public PackageManifest ActiveManifest { set; get; }

        void IPlayMode.UpdatePlayMode()
        {
            if (WebServerFileSystem != null)
                WebServerFileSystem.OnUpdate();

            if (WebRemoteFileSystem != null)
                WebRemoteFileSystem.OnUpdate();
        }

        RequestPackageVersionOperation IPlayMode.RequestPackageVersionAsync(bool appendTimeTicks, int timeout)
        {
            if (WebRemoteFileSystem != null)
            {
                var operation = new RequestPackageVersionImplOperation(WebRemoteFileSystem, appendTimeTicks, timeout);
                OperationSystem.StartOperation(PackageName, operation);
                return operation;
            }
            else
            {
                var operation = new RequestPackageVersionImplOperation(WebServerFileSystem, appendTimeTicks, timeout);
                OperationSystem.StartOperation(PackageName, operation);
                return operation;
            }
        }
        UpdatePackageManifestOperation IPlayMode.UpdatePackageManifestAsync(string packageVersion, int timeout)
        {
            if (WebServerFileSystem is DefaultWebServerFileSystem defaultWebServerFileSystem &&
                packageVersion == defaultWebServerFileSystem.BuildInPackageVersion)
            {
                var operation = new UpdatePackageManifestImplOperation(this, WebServerFileSystem, packageVersion, timeout); ;
                OperationSystem.StartOperation(PackageName, operation);
                return operation;
            }
            if (WebRemoteFileSystem != null)
            {
                var operation = new UpdatePackageManifestImplOperation(this, WebRemoteFileSystem, packageVersion, timeout); ;
                OperationSystem.StartOperation(PackageName, operation);
                return operation;
            }
            else
            {
                var operation = new UpdatePackageManifestImplOperation(this, WebServerFileSystem, packageVersion, timeout); ;
                OperationSystem.StartOperation(PackageName, operation);
                return operation;
            }
        }
        PreDownloadContentOperation IPlayMode.PreDownloadContentAsync(string packageVersion, int timeout)
        {
            var operation = new WebPlayModePreDownloadContentOperation(this);
            OperationSystem.StartOperation(PackageName, operation);
            return operation;
        }
        ClearCacheFilesOperation IPlayMode.ClearCacheFilesAsync(string clearMode, object clearParam)
        {
            var operation = new ClearCacheFilesImplOperation(this, WebServerFileSystem, WebRemoteFileSystem, null, clearMode, clearParam);
            OperationSystem.StartOperation(PackageName, operation);
            return operation;
        }

        ResourceDownloaderOperation IPlayMode.CreateResourceDownloaderByAll(int downloadingMaxNumber, int failedTryAgain, int timeout)
        {
            List<BundleInfo> downloadList = PlayModeHelper.GetDownloadListByAll(ActiveManifest, WebServerFileSystem, WebRemoteFileSystem);
            var operation = new ResourceDownloaderOperation(PackageName, downloadList, downloadingMaxNumber, failedTryAgain, timeout);
            return operation;
        }
        ResourceDownloaderOperation IPlayMode.CreateResourceDownloaderByTags(string[] tags, int downloadingMaxNumber, int failedTryAgain, int timeout)
        {
            List<BundleInfo> downloadList = PlayModeHelper.GetDownloadListByTags(ActiveManifest, tags, WebServerFileSystem, WebRemoteFileSystem);
            var operation = new ResourceDownloaderOperation(PackageName, downloadList, downloadingMaxNumber, failedTryAgain, timeout);
            return operation;
        }
        ResourceDownloaderOperation IPlayMode.CreateResourceDownloaderByPaths(AssetInfo[] assetInfos, int downloadingMaxNumber, int failedTryAgain, int timeout)
        {
            List<BundleInfo> downloadList = PlayModeHelper.GetDownloadListByPaths(ActiveManifest, assetInfos, WebServerFileSystem, WebRemoteFileSystem);
            var operation = new ResourceDownloaderOperation(PackageName, downloadList, downloadingMaxNumber, failedTryAgain, timeout);
            return operation;
        }
        ResourceUnpackerOperation IPlayMode.CreateResourceUnpackerByAll(int upackingMaxNumber, int failedTryAgain, int timeout)
        {
            List<BundleInfo> unpcakList = PlayModeHelper.GetUnpackListByAll(ActiveManifest, WebServerFileSystem, WebRemoteFileSystem);
            var operation = new ResourceUnpackerOperation(PackageName, unpcakList, upackingMaxNumber, failedTryAgain, timeout);
            return operation;
        }
        ResourceUnpackerOperation IPlayMode.CreateResourceUnpackerByTags(string[] tags, int upackingMaxNumber, int failedTryAgain, int timeout)
        {
            List<BundleInfo> unpcakList = PlayModeHelper.GetUnpackListByTags(ActiveManifest, tags, WebServerFileSystem, WebRemoteFileSystem);
            var operation = new ResourceUnpackerOperation(PackageName, unpcakList, upackingMaxNumber, failedTryAgain, timeout);
            return operation;
        }
        ResourceImporterOperation IPlayMode.CreateResourceImporterByFilePaths(string[] filePaths, int importerMaxNumber, int failedTryAgain, int timeout)
        {
            List<BundleInfo> importerList = PlayModeHelper.GetImporterListByFilePaths(ActiveManifest, filePaths, WebServerFileSystem, WebRemoteFileSystem);
            var operation = new ResourceImporterOperation(PackageName, importerList, importerMaxNumber, failedTryAgain, timeout);
            return operation;
        }
        #endregion

        #region IBundleQuery接口
        private BundleInfo CreateBundleInfo(PackageBundle packageBundle, AssetInfo assetInfo)
        {
            if (packageBundle == null)
                throw new Exception("Should never get here !");

            if (WebServerFileSystem != null && WebServerFileSystem.Belong(packageBundle))
            {
                BundleInfo bundleInfo = new BundleInfo(WebServerFileSystem, packageBundle);
                return bundleInfo;
            }

            if (WebRemoteFileSystem != null && WebRemoteFileSystem.Belong(packageBundle))
            {
                BundleInfo bundleInfo = new BundleInfo(WebRemoteFileSystem, packageBundle);
                return bundleInfo;
            }

            throw new Exception($"Can not found belong file system : {packageBundle.BundleName}");
        }
        BundleInfo IBundleQuery.GetMainBundleInfo(AssetInfo assetInfo)
        {
            if (assetInfo.IsInvalid)
                throw new Exception("Should never get here !");

            // 注意：如果清单里未找到资源包会抛出异常！
            var packageBundle = ActiveManifest.GetMainPackageBundle(assetInfo.AssetPath);
            return CreateBundleInfo(packageBundle, assetInfo);
        }
        BundleInfo[] IBundleQuery.GetDependBundleInfos(AssetInfo assetInfo)
        {
            if (assetInfo.IsInvalid)
                throw new Exception("Should never get here !");

            // 注意：如果清单里未找到资源包会抛出异常！
            var depends = ActiveManifest.GetAllDependencies(assetInfo.AssetPath);
            List<BundleInfo> result = new List<BundleInfo>(depends.Length);
            foreach (var packageBundle in depends)
            {
                BundleInfo bundleInfo = CreateBundleInfo(packageBundle, assetInfo);
                result.Add(bundleInfo);
            }
            return result.ToArray();
        }
        string IBundleQuery.GetMainBundleName(AssetInfo assetInfo)
        {
            if (assetInfo.IsInvalid)
                throw new Exception("Should never get here !");

            // 注意：如果清单里未找到资源包会抛出异常！
            var packageBundle = ActiveManifest.GetMainPackageBundle(assetInfo.AssetPath);
            return packageBundle.BundleName;
        }
        string[] IBundleQuery.GetDependBundleNames(AssetInfo assetInfo)
        {
            if (assetInfo.IsInvalid)
                throw new Exception("Should never get here !");

            // 注意：如果清单里未找到资源包会抛出异常！
            var depends = ActiveManifest.GetAllDependencies(assetInfo.AssetPath);
            List<string> result = new List<string>(depends.Length);
            foreach (var packageBundle in depends)
            {
                result.Add(packageBundle.BundleName);
            }
            return result.ToArray();
        }
        bool IBundleQuery.ManifestValid()
        {
            return ActiveManifest != null;
        }
        #endregion
    }
}