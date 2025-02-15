using System;
using System.Collections.Generic;
using System.IO;
using YooAsset;

namespace TaoTie
{
    public class PackageManager : IManager
    {
        public static PackageManager Instance;

        private PackageConfig config;
        int downloadingMaxNum = 10;
        int failedTryAgain = 2;
        private int timeOut = 8;

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
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].State == PackageState.Initialized)
                {
                    YooAssetsMgr.Instance.ForceUnloadAllAssets(list[i].Name);
                }
                else if (list[i].State == PackageState.Downloading)
                {
                    list[i].DownloaderOperation?.CancelDownload();
                }
            }
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

        public async ETTask<bool> UpdatePackageManifest(string name)
        {
            if(name == YooAssetsMgr.DefaultName)
                return true;
            var packageInfo = GetPackageInfo(name);
            if (packageInfo == null) return false;
            if (packageInfo.State == PackageState.UpdateManifest)
            {
                int max = GetPackageMaxVer(name);
                packageInfo.UpdatePackageManifestOperation =
                    YooAssetsMgr.Instance.UpdatePackageManifestAsync(max.ToString(), false, 10, name);
                await packageInfo.UpdatePackageManifestOperation.Task;
                if (packageInfo.UpdatePackageManifestOperation.Status != EOperationStatus.Succeed)
                {
                    Log.Error(packageInfo.UpdatePackageManifestOperation.Error);
                    packageInfo.State = PackageState.UpdateManifest;
                    return false;
                }
                packageInfo.DownloaderOperation =
                    YooAssetsMgr.Instance.CreateResourceDownloader(downloadingMaxNum, failedTryAgain, 10, name);
                if (packageInfo.DownloaderOperation.TotalDownloadCount == 0)
                {
                    packageInfo.State = PackageState.Initialized;
                }
                else
                {
                    packageInfo.State = PackageState.NeedUpdate;
                    packageInfo.NeedDownloadSize = packageInfo.DownloaderOperation.TotalDownloadBytes;
                }
            }

            return true;
        }

        public async ETTask<bool> UpdatePackageInfo(string name, Action<float> onProgress = null)
        {
            if(name == YooAssetsMgr.DefaultName)
                return true;
            var packageInfo = GetPackageInfo(name);
            if (packageInfo == null) return false;
            if (packageInfo.State == PackageState.NeedUpdate || packageInfo.State == PackageState.UnDownload)
            {
                packageInfo.State = PackageState.Downloading;
                if (YooAssetsMgr.Instance.PlayMode == EPlayMode.HostPlayMode)
                {
                    packageInfo.DownloaderOperation.OnDownloadProgressCallback = (a, b, c, d) =>
                    {
                        float progress = (float) d / c;
                        Messager.Instance.Broadcast(0, MessageId.Package_DownLoading, name, progress);
                        onProgress?.Invoke(progress);
                        if (packageInfo.DownloadSize != c || c == d)
                        {
                            packageInfo.DownloadSize = c;
                        }
                    };

                    Log.Info("UpdatePackageInfo DownloadContent begin");
                    packageInfo.DownloaderOperation.BeginDownload();
                    await packageInfo.DownloaderOperation.Task;
                    if (packageInfo.DownloaderOperation.Status != EOperationStatus.Succeed)
                    {
                        Log.Error(packageInfo.DownloaderOperation.Error);
                        packageInfo.State = PackageState.UpdateManifest;
                        packageInfo.DownloaderOperation.CancelDownload();
                        Messager.Instance.Broadcast(0, MessageId.Package_DownLoad, name, false);
                        // UIToastManager.Instance.ShowToast(I18NKey.Net_Download_Error).Coroutine();
                        return false;
                    }

                    packageInfo.UpdatePackageManifestOperation.SavePackageVersion();
                    Log.Info("UpdatePackageInfo DownloadContent Success");
                    packageInfo.State = PackageState.Initialized;
                    Messager.Instance.Broadcast(0, MessageId.Package_DownLoad, name, true);
                    return true;
                }
                
                if (YooAssetsMgr.Instance.PlayMode == EPlayMode.EditorSimulateMode)
                {
                    int i = 0;
                    while (i<100)
                    {
                        await TimerManager.Instance.WaitAsync(50);
                        i++;
                        float progress = (float) i / 100;
                        Messager.Instance.Broadcast(0, MessageId.Package_DownLoading, name, progress);
                        onProgress?.Invoke(progress);
                    }
                    Log.Info("UpdatePackageInfo DownloadContent Success");
                    packageInfo.State = PackageState.Initialized;
                    Messager.Instance.Broadcast(0, MessageId.Package_DownLoad, name, true);
                    return true;
                }

                if (YooAssetsMgr.Instance.PlayMode == EPlayMode.OfflinePlayMode)
                {
                    packageInfo.State = PackageState.Initialized;
                    Messager.Instance.Broadcast(0, MessageId.Package_DownLoad, name, true);
                    return true;
                }
            } 

            return true;
        }

        public void ReSetUpdatePackageInfoProgress(string name, Action<float> onProgress = null)
        {
            if(name == YooAssetsMgr.DefaultName)
                return ;
            var packageInfo = GetPackageInfo(name);
            if (packageInfo == null) return ;
            if (packageInfo.State == PackageState.Downloading)
            {
                packageInfo.DownloaderOperation.OnDownloadProgressCallback = (a, b, c, d) =>
                {
                    float progress = (float) d / c;
                    Messager.Instance.Broadcast(0,MessageId.Package_DownLoading,name,progress);
                    onProgress?.Invoke(progress);
                };
            }
        }
        private async ETTask InitAsync()
        {
            var op = YooAssetsMgr.Instance.DefaultPackage.LoadRawFileAsync("packageConfig.bytes");
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
                    InitPackage(item.Key).Coroutine();//只能await
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
            
            int max = GetPackageMaxVer(name);
            packageInfo.MaxVer = max;

            if (YooAssetsMgr.Instance.PlayMode != EPlayMode.EditorSimulateMode && max < 0)
            {
                packageInfo.State = PackageState.Error;
                Log.Error("不存在包" + name);
                return;
            }

            if (YooAssetsMgr.Instance.PlayMode == EPlayMode.HostPlayMode)
            {
                var package = await YooAssetsMgr.Instance.GetPackage(name);
                var ver = GetPackageVersion(package);
                Log.Info(name + " ver:" + ver);
                packageInfo.Ver = ver;
                if (ver < 0 || max >= ver)//确保版本和主包代码版本对应
                {
                    packageInfo.UpdatePackageManifestOperation =
                        YooAssetsMgr.Instance.UpdatePackageManifestAsync(max.ToString(), false, timeOut, name);
                    await packageInfo.UpdatePackageManifestOperation.Task;
                    if (packageInfo.UpdatePackageManifestOperation.Status != EOperationStatus.Succeed)
                    {
                        Log.Error(packageInfo.UpdatePackageManifestOperation.Error);
                        packageInfo.State = PackageState.UpdateManifest;
                        return;
                    }

                    packageInfo.DownloaderOperation =
                        YooAssetsMgr.Instance.CreateResourceDownloader(downloadingMaxNum, failedTryAgain, timeOut, name);
                    if (packageInfo.DownloaderOperation.TotalDownloadCount == 0)
                    {
                        packageInfo.State = PackageState.Initialized;
                    }
                    else
                    {
                        packageInfo.State = ver < 0 ? PackageState.UnDownload : PackageState.NeedUpdate;
                        packageInfo.NeedDownloadSize = packageInfo.DownloaderOperation.TotalDownloadBytes;
                    }
                }
                else
                {
                    packageInfo.State = PackageState.Initialized;
                }
            }
            else if (YooAssetsMgr.Instance.PlayMode == EPlayMode.OfflinePlayMode)
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