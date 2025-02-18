using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace YooAsset.Editor
{
    public class ManifestContext : IContextObject
    {
        internal PackageManifest Manifest;
    }

    public abstract class TaskCreateManifest
    {
        private readonly Dictionary<string, int> _cachedBundleIndexIDs = new Dictionary<string, int>(10000);
        private readonly Dictionary<int, HashSet<string>> _cacheBundleTags = new Dictionary<int, HashSet<string>>(10000);

        /// <summary>
        /// 创建补丁清单文件到输出目录
        /// </summary>
        protected void CreateManifestFile(bool processBundleDepends, bool processBundleTags, BuildContext context)
        {
            var buildMapContext = context.GetContextObject<BuildMapContext>();
            var buildParametersContext = context.GetContextObject<BuildParametersContext>();
            var buildParameters = buildParametersContext.Parameters;
            string packageOutputDirectory = buildParametersContext.GetPackageOutputDirectory();

            // 检测资源包哈希冲突
            CheckBundleHashConflict(buildMapContext);

            // 创建新补丁清单
            PackageManifest manifest = new PackageManifest();
            manifest.FileVersion = YooAssetSettings.ManifestFileVersion;
            manifest.EnableAddressable = buildMapContext.Command.EnableAddressable;
            manifest.LocationToLower = buildMapContext.Command.LocationToLower;
            manifest.IncludeAssetGUID = buildMapContext.Command.IncludeAssetGUID;
            manifest.OutputNameStyle = (int)buildParameters.FileNameStyle;
            manifest.BuildBundleType = buildParameters.BuildBundleType;
            manifest.BuildPipeline = buildParameters.BuildPipeline;
            manifest.PackageName = buildParameters.PackageName;
            manifest.PackageVersion = buildParameters.PackageVersion;
            manifest.PackageNote = buildParameters.PackageNote;
            manifest.AssetList = CreatePackageAssetList(buildMapContext);
            manifest.BundleList = CreatePackageBundleList(buildMapContext);

            // 处理资源清单的ID数据
            ProcessPacakgeIDs(manifest);

            // 处理资源包的依赖列表
            if (processBundleDepends)
                ProcessBundleDepends(context, manifest);

            // 处理资源包的标签集合
            if (processBundleTags)
                ProcessBundleTags(manifest);

            // 创建补丁清单文本文件
            {
                string fileName = YooAssetSettingsData.GetManifestJsonFileName(buildParameters.PackageName, buildParameters.PackageVersion);
                string filePath = $"{packageOutputDirectory}/{fileName}";
                ManifestTools.SerializeToJson(filePath, manifest);
                BuildLogger.Log($"Create package manifest file: {filePath}");
            }

            // 创建补丁清单二进制文件
            string packageHash;
            {
                string fileName = YooAssetSettingsData.GetManifestBinaryFileName(buildParameters.PackageName, buildParameters.PackageVersion);
                string filePath = $"{packageOutputDirectory}/{fileName}";
                ManifestTools.SerializeToBinary(filePath, manifest);
                packageHash = HashUtility.FileCRC32(filePath);
                BuildLogger.Log($"Create package manifest file: {filePath}");

                ManifestContext manifestContext = new ManifestContext();
                byte[] bytesData = FileUtility.ReadAllBytes(filePath);
                manifestContext.Manifest = ManifestTools.DeserializeFromBinary(bytesData);
                context.SetContextObject(manifestContext);
            }

            // 创建补丁清单哈希文件
            {
                string fileName = YooAssetSettingsData.GetPackageHashFileName(buildParameters.PackageName, buildParameters.PackageVersion);
                string filePath = $"{packageOutputDirectory}/{fileName}";
                FileUtility.WriteAllText(filePath, packageHash);
                BuildLogger.Log($"Create package manifest hash file: {filePath}");
            }

            // 创建补丁清单版本文件
            {
                string fileName = YooAssetSettingsData.GetPackageVersionFileName(buildParameters.PackageName);
                string filePath = $"{packageOutputDirectory}/{fileName}";
                FileUtility.WriteAllText(filePath, buildParameters.PackageVersion);
                BuildLogger.Log($"Create package manifest version file: {filePath}");
            }
        }

        /// <summary>
        /// 检测资源包哈希冲突
        /// </summary>
        private void CheckBundleHashConflict(BuildMapContext buildMapContext)
        {
            // 说明：在特殊情况下，例如某些文件加密算法会导致加密后的文件哈希值冲突！
            // 说明：二进制完全相同的原生文件也会冲突！
            HashSet<string> guids = new HashSet<string>();
            foreach (var bundleInfo in buildMapContext.Collection)
            {
                if (guids.Contains(bundleInfo.PackageFileHash))
                {
                    string message = BuildLogger.GetErrorMessage(ErrorCode.BundleHashConflict, $"Bundle hash conflict : {bundleInfo.BundleName}");
                    throw new Exception(message);
                }
                else
                {
                    guids.Add(bundleInfo.PackageFileHash);
                }
            }
        }

        /// <summary>
        /// 获取资源包的依赖集合
        /// </summary>
        protected abstract string[] GetBundleDepends(BuildContext context, string bundleName);

        /// <summary>
        /// 创建资源对象列表
        /// </summary>
        private List<PackageAsset> CreatePackageAssetList(BuildMapContext buildMapContext)
        {
            List<PackageAsset> result = new List<PackageAsset>(1000);
            foreach (var bundleInfo in buildMapContext.Collection)
            {
                var assetInfos = bundleInfo.GetAllManifestAssetInfos();
                foreach (var assetInfo in assetInfos)
                {
                    PackageAsset packageAsset = new PackageAsset();
                    packageAsset.Address = buildMapContext.Command.EnableAddressable ? assetInfo.Address : string.Empty;
                    packageAsset.AssetPath = assetInfo.AssetInfo.AssetPath;
                    packageAsset.AssetGUID = buildMapContext.Command.IncludeAssetGUID ? assetInfo.AssetInfo.AssetGUID : string.Empty;
                    packageAsset.AssetTags = assetInfo.AssetTags.ToArray();
                    packageAsset.BundleNameInEditor = assetInfo.BundleName;
                    result.Add(packageAsset);
                }
            }

            // 按照AssetPath排序
            result.Sort((a, b) => a.AssetPath.CompareTo(b.AssetPath));
            return result;
        }

        /// <summary>
        /// 创建资源包列表
        /// </summary>
        private List<PackageBundle> CreatePackageBundleList(BuildMapContext buildMapContext)
        {
            List<PackageBundle> result = new List<PackageBundle>(1000);
            foreach (var bundleInfo in buildMapContext.Collection)
            {
                var packageBundle = bundleInfo.CreatePackageBundle();
                result.Add(packageBundle);
            }

            // 按照BundleName排序
            result.Sort((a, b) => a.BundleName.CompareTo(b.BundleName));
            return result;
        }

        /// <summary>
        /// 处理资源清单的ID数据
        /// </summary>
        private void ProcessPacakgeIDs(PackageManifest manifest)
        {
            // 注意：优先缓存资源包索引
            for (int index = 0; index < manifest.BundleList.Count; index++)
            {
                string bundleName = manifest.BundleList[index].BundleName;
                _cachedBundleIndexIDs.Add(bundleName, index);
            }

            foreach (var packageAsset in manifest.AssetList)
            {
                string bundleName = packageAsset.BundleNameInEditor;
                packageAsset.BundleID = GetCachedBundleIndexID(bundleName);
            }
        }

        /// <summary>
        /// 处理资源包的依赖集合
        /// </summary>
        private void ProcessBundleDepends(BuildContext context, PackageManifest manifest)
        {
            // 查询引擎生成的资源包依赖关系，然后记录到清单
            foreach (var packageBundle in manifest.BundleList)
            {
                int mainBundleID = GetCachedBundleIndexID(packageBundle.BundleName);
                var depends = GetBundleDepends(context, packageBundle.BundleName);
                List<int> dependIDs = new List<int>(depends.Length);
                foreach (var dependBundleName in depends)
                {
                    int bundleID = GetCachedBundleIndexID(dependBundleName);
                    if (bundleID != mainBundleID)
                        dependIDs.Add(bundleID);
                }
                packageBundle.DependIDs = dependIDs.ToArray();
            }
        }

        /// <summary>
        /// 处理资源包的标签集合
        /// </summary>
        private void ProcessBundleTags(PackageManifest manifest)
        {
            // 将主资源的标签信息传染给其依赖的资源包集合
            foreach (var packageAsset in manifest.AssetList)
            {
                var assetTags = packageAsset.AssetTags;
                int bundleID = packageAsset.BundleID;
                CacheBundleTags(bundleID, assetTags);

                var packageBundle = manifest.BundleList[bundleID];
                if (packageBundle.DependIDs != null)
                {
                    foreach (var dependBundleID in packageBundle.DependIDs)
                    {
                        CacheBundleTags(dependBundleID, assetTags);
                    }
                }
            }

            for (int index = 0; index < manifest.BundleList.Count; index++)
            {
                var packageBundle = manifest.BundleList[index];
                if (_cacheBundleTags.TryGetValue(index, out var value))
                {
                    packageBundle.Tags = value.ToArray();
                }
                else
                {
                    // 注意：SBP构建管线会自动剔除一些冗余资源的引用关系，导致游离资源包没有被任何主资源包引用。
                    string warning = BuildLogger.GetErrorMessage(ErrorCode.FoundStrayBundle, $"Found stray bundle ! Bundle ID : {index} Bundle name : {packageBundle.BundleName}");
                    BuildLogger.Warning(warning);
                }
            }
        }
        private void CacheBundleTags(int bundleID, string[] assetTags)
        {
            if (_cacheBundleTags.ContainsKey(bundleID) == false)
                _cacheBundleTags.Add(bundleID, new HashSet<string>());

            foreach (var assetTag in assetTags)
            {
                if (_cacheBundleTags[bundleID].Contains(assetTag) == false)
                    _cacheBundleTags[bundleID].Add(assetTag);
            }
        }

        /// <summary>
        /// 获取缓存的资源包的索引ID
        /// </summary>
        private int GetCachedBundleIndexID(string bundleName)
        {
            if (_cachedBundleIndexIDs.TryGetValue(bundleName, out int value) == false)
            {
                throw new Exception($"Should never get here ! Not found bundle index ID : {bundleName}");
            }
            return value;
        }
    }
}