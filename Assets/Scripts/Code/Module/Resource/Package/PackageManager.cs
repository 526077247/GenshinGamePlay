using System.Collections.Generic;
using YooAsset;

namespace TaoTie
{
    public class PackageManager: IManager
    {
        public static PackageManager Instance;

        private PackageConfig config;
        int downloadingMaxNum = 10;
        int failedTryAgain = 3;

        private List<PackageInfo> list;
        private Dictionary<string, PackageInfo> dict;

        public void Init()
        {
            Instance = this;
            list = new List<PackageInfo>();
            dict = new Dictionary<string, PackageInfo>();
            InitAsync().Coroutine();
        }

        public void Destroy()
        {
            Instance = null;
            list = null;
            dict = null;
        }

        public List<PackageInfo> GetAllList()
        {
            return list;
        }

        public PackageInfo GetPackageInfo(string name)
        {
            if (dict.TryGetValue(name, out var packageInfo))
            {
                return packageInfo;
            }

            return null;
        }

        private async ETTask InitAsync()
        {
            var op = YooAssetsMgr.Instance.DefaultPackage.LoadRawFileSync("packageConfig.bytes");
            await op.Task;
            var conf = op.GetRawFileText();
            config = JsonHelper.FromJson<PackageConfig>(conf);
            op.Release();
            if (config?.packageVer != null)
            {
                foreach (var item in config.packageVer)
                {
                    InitPackage(item.Key).Coroutine();
                }
            }
        }

        private async ETTask<PackageInfo> InitPackage(string name)
        {
            if (dict.TryGetValue(name, out var packageInfo))
            {
                return packageInfo;
            }
            packageInfo = new PackageInfo();
            packageInfo.Name = name;
            var package = await YooAssetsMgr.Instance.GetPackage(name);
            int max = GetPackageMaxVer(name);
            var ver = GetPackageVersion(package);
            packageInfo.MaxVer = max;
            packageInfo.Ver = ver;
            if (max < 0)
            {
                packageInfo.State = PackageState.Error;
                Log.Error("不存在包" + name);
                return packageInfo;
            }

            if (ver < 0)
            {
                packageInfo.State = PackageState.UnDownload;
                return packageInfo;
            }

            if (max > ver)
            {
                packageInfo.State = PackageState.NeedUpdate;
                var operation =
                    YooAssetsMgr.Instance.UpdatePackageManifestAsync(max.ToString(), true, 30, name);
                await operation.Task;
                var dl = 
                    YooAssetsMgr.Instance.CreateResourceDownloader(downloadingMaxNum, failedTryAgain, 30, name);
                packageInfo.NeedDownloadSize = dl.TotalDownloadBytes;
                operation =
                    YooAssetsMgr.Instance.UpdatePackageManifestAsync(ver.ToString(), true, 30, name);
                await operation.Task;
            }
            else
                packageInfo.State = PackageState.Initialized;
            dict.Add(name,packageInfo);
            list.Add(packageInfo);
            return packageInfo;
        }

        private int GetPackageVersion(ResourcePackage package)
        {
            string res = package.GetPackageVersion();
            if (int.TryParse(res, out var ver))
            {
                return ver;
            }
            return -1;
        }
        private int GetPackageMaxVer(string name)
        {
            if (config == null || config.packageVer==null) return -1;
            if (config.packageVer.TryGetValue(name, out var res))
            {
                return res;
            }

            return -1;
        }
    }
}