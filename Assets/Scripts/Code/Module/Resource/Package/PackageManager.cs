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

        public async ETTask<bool> UpdatePackageInfo(string name, Action<float> onProgress = null)
        {
            var packageInfo = GetPackageInfo(name);
            if (packageInfo.State == PackageState.NeedUpdate || packageInfo.State == PackageState.UnDownload)
            {
                packageInfo.State = PackageState.Downloading;
                var res = await YooAssetsMgr.Instance.UpdatePackageManifestAsync(packageInfo.MaxVer.ToString(), true, 30, name);
                Log.Info("UpdatePackageInfo UpdatePackageManifestAsync begin");
                if (!res)
                {
                    packageInfo.State = PackageState.NeedUpdate;
                    return false;
                }

                Log.Info("UpdatePackageInfo UpdatePackageManifestAsync Success");
                ETTask<bool> downloadTask = ETTask<bool>.Create(true);
                packageInfo.DownloaderOperation.OnDownloadOverCallback += (a) => { downloadTask?.SetResult(a); };
                if (onProgress != null)
                {
                    packageInfo.DownloaderOperation.OnDownloadProgressCallback = (a, b, c, d) =>
                    {
                        onProgress((float) d / c);
                    };
                }
                Log.Info("UpdatePackageInfo DownloadContent begin");
                packageInfo.DownloaderOperation.BeginDownload();
                
                bool result = await downloadTask;
                if (!result)
                {
                    packageInfo.State = PackageState.NeedUpdate;
                    return false;
                }

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
            Log.Info(name+" ver:"+ver);
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
                if (ver < 0 || max > ver)
                {
                    packageInfo.State = ver < 0 ? PackageState.UnDownload : PackageState.NeedUpdate;
 
                    var res = await YooAssetsMgr.Instance.UpdatePackageManifestAsync(max.ToString(), true, 30, name);
                    if (!res)
                    {
                        packageInfo.State = PackageState.Error;
                        return;
                    }

                    packageInfo.DownloaderOperation =
                        YooAssetsMgr.Instance.CreateResourceDownloader(downloadingMaxNum, failedTryAgain, 30, name);
                    packageInfo.NeedDownloadSize = packageInfo.DownloaderOperation.TotalDownloadBytes;
                    if (ver > 0)
                    {
                        res = await YooAssetsMgr.Instance.UpdatePackageManifestAsync(ver.ToString(), true, 30, name);
                        if (!res)
                        {
                            packageInfo.State = PackageState.Error;
                        }
                    }
                }
                packageInfo.State = PackageState.Initialized;
            }
            else if (YooAssetsMgr.Instance.PlayMode == EPlayMode.EditorSimulateMode
                     ||YooAssetsMgr.Instance.PlayMode == EPlayMode.OfflinePlayMode)
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
            if (config == null || config.packageVer==null) return -1;
            if (config.packageVer.TryGetValue(name, out var res))
            {
                return res;
            }

            return -1;
        }
    }
}