using System;
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
        private PackageInfo defaultPackage;

        public void Init()
        {
            Instance = this;
            list = new List<PackageInfo>();
            dict = new Dictionary<string, PackageInfo>();
            defaultPackage = new PackageInfo()
            {
                Name = YooAssetsMgr.DefaultName,
                State = PackageState.Initialized
            };
            InitAsync().Coroutine();
        }

        public void Destroy()
        {
            defaultPackage = null;
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
            if (name == YooAssetsMgr.DefaultName)
            {
                return defaultPackage;
            }
            if (dict.TryGetValue(name, out var packageInfo))
            {
                return packageInfo;
            }

            return null;
        }

        public async ETTask<bool> UpdatePackageInfo(string name, Action<float> onProgress = null)
        {
            if(name == YooAssetsMgr.DefaultName)
                return true;
            var packageInfo = GetPackageInfo(name);
            if (packageInfo.State == PackageState.NeedUpdate || packageInfo.State == PackageState.UnDownload)
            {
                packageInfo.State = PackageState.Downloading;
                if (onProgress != null)
                {
                    packageInfo.DownloaderOperation.OnDownloadProgressCallback = (a, b, c, d) =>
                    {
                        onProgress((float) d / c);
                    };
                }

                Log.Info("UpdatePackageInfo DownloadContent begin");
                packageInfo.DownloaderOperation.BeginDownload();
                await packageInfo.DownloaderOperation.Task;

                if (packageInfo.DownloaderOperation.Status != EOperationStatus.Succeed)
                {
                    Log.Error(packageInfo.DownloaderOperation.Error);
                    packageInfo.State = PackageState.NeedUpdate;
                    return false;
                }

                packageInfo.UpdatePackageManifestOperation.SavePackageVersion();
                Log.Info("UpdatePackageInfo DownloadContent Success");
                packageInfo.State = PackageState.Initialized;
                return true;
            }

            return true;
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
                // using (ListComponent<ETTask> task = ListComponent<ETTask>.Create())
                // {
                //     foreach (var item in config.packageVer)
                //     {
                //         task.Add(InitPackage(item.Key));
                //     }
                //     await ETTaskHelper.WaitAll(task);
                //     Log.Info(JsonHelper.ToJson(list));
                // }
                foreach (var item in config.packageVer)
                {
                    InitPackage(item.Key).Coroutine();
                }
            }
        }

        private async ETTask InitPackage(string name)
        {
            if (dict.TryGetValue(name, out var packageInfo))
            {
                return;
            }

            packageInfo = new PackageInfo();
            dict.Add(name, packageInfo);
            list.Add(packageInfo);
            packageInfo.Name = name;
            var package = await YooAssetsMgr.Instance.GetPackage(name);
            int max = GetPackageMaxVer(name);
            var ver = GetPackageVersion(package);
            Log.Info(name + " ver:" + ver);
            packageInfo.MaxVer = max;
            packageInfo.Ver = ver;
            if (max < 0)
            {
                packageInfo.State = PackageState.Error;
                Log.Error("不存在包" + name);
                return;
            }

            if (YooAssetsMgr.Instance.PlayMode == EPlayMode.HostPlayMode)
            {
                if (ver < 0 || max > ver)//确保版本和主包代码版本对应
                {
                    packageInfo.State = ver < 0 ? PackageState.UnDownload : PackageState.NeedUpdate;

                    packageInfo.UpdatePackageManifestOperation =
                        YooAssetsMgr.Instance.UpdatePackageManifestAsync(max.ToString(), false, 30, name);
                    await packageInfo.UpdatePackageManifestOperation.Task;
                    if (packageInfo.UpdatePackageManifestOperation.Status != EOperationStatus.Succeed)
                    {
                        Log.Error(packageInfo.UpdatePackageManifestOperation.Error);
                        packageInfo.State = PackageState.Error;
                        return;
                    }

                    packageInfo.DownloaderOperation =
                        YooAssetsMgr.Instance.CreateResourceDownloader(downloadingMaxNum, failedTryAgain, 30, name);
                    packageInfo.NeedDownloadSize = packageInfo.DownloaderOperation.TotalDownloadBytes;
                }
                else
                {
                    packageInfo.State = PackageState.Initialized;
                }
            }
            else if (YooAssetsMgr.Instance.PlayMode == EPlayMode.EditorSimulateMode
                     || YooAssetsMgr.Instance.PlayMode == EPlayMode.OfflinePlayMode)
            {
                packageInfo.State = PackageState.Initialized;
            }
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
            if (config == null || config.packageVer == null) return -1;
            if (config.packageVer.TryGetValue(name, out var res))
            {
                return res;
            }

            return -1;
        }
    }
}