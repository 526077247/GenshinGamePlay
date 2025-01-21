using UnityEngine;
using YooAsset;

namespace TaoTie
{
    public class MainPackageUpdateProcess: UpdateProcess
    {
        private bool forceUpdate;
        private string[] packageName;

        public MainPackageUpdateProcess(params string[] packageName)
        {
            this.packageName = packageName;
        }

        public override ETTask<UpdateRes> Process(UpdateTask task)
        {
            if (packageName == null || packageName.Length <= 0)
            {
                Log.Info("ProcessOnlyMain");
                return ProcessOnlyMain(task);
            }
            else
            {
                Log.Info("ProcessWithPackage");
                return ProcessWithPackage(task);
            }

        }

        private async ETTask<UpdateRes> ProcessOnlyMain(UpdateTask task)
        {
            var channel = YooAssetsMgr.Instance.CdnConfig.Channel;
            
            bool isAppMaxVer = !ServerConfigManager.Instance.FindMaxUpdateResVerThisAppVer(channel, task.AppVer, out var maxAppResVer);

            forceUpdate = Define.ForceUpdate;
            var verInfo = ServerConfigManager.Instance.GetResVerInfo(channel, YooAssetsMgr.Instance.Config.Resver);
            if (verInfo != null && verInfo.force_update == 1)
                forceUpdate = true;
            
            var maxVer = ServerConfigManager.Instance.FindMaxUpdateResVer(channel, "", maxAppResVer);
            if (maxVer < 0)
            {
                Log.Info("CheckResUpdate No Max Ver Channel = " + channel + " ");
                return UpdateRes.Over;
            }

            if (isAppMaxVer)
            {
                maxVer = YooAssetsMgr.Instance.Config.Resver;
            }

            // 编辑器下跳过。
            if (YooAssetsMgr.Instance.PlayMode != EPlayMode.HostPlayMode)
            {
                Log.Info("非网络运行模式");
                return UpdateRes.Over;
            }
            // 更新补丁清单
            Log.Info("更新补丁清单 "+maxVer);

            var op = YooAssetsMgr.Instance.UpdatePackageManifestAsync(maxVer.ToString(), false, task.TimeOut, null);
            await op.Task;
            if(op.Status!= EOperationStatus.Succeed)
            {
                Log.Error(op.Error);
                return await UpdateFail(task, maxVer != YooAssetsMgr.Instance.Config.Resver);
            }

            Log.Info("创建补丁下载器.");
            
            var downloader = YooAssetsMgr.Instance.CreateResourceDownloader(task.DownloadingMaxNum, task.FailedTryAgain, task.TimeOut, null);
            if (downloader.TotalDownloadCount == 0)
            {
                Log.Info("没有发现需要下载的资源");
                return UpdateRes.Over;
            }
            
            //获取需要更新的大小
            var size = downloader.TotalDownloadBytes;
            //提示给用户
            Log.Info("downloadSize " + size);
            double sizeMb = size / (1024f * 1024f);
            Log.Info("CheckResUpdate res size_mb is " + sizeMb);//不屏蔽
            if (sizeMb > 0 && sizeMb < 0.01) sizeMb = 0.01;
           

            var ct = I18NManager.Instance.I18NGetParamText("Update_Info", sizeMb.ToString("0.00"));
            var btnState = await task.ShowMsgBoxView(ct, "Global_Btn_Confirm",
                forceUpdate ? "Btn_Exit" : "Update_Skip");
            if (!btnState)
            {
                if (forceUpdate)
                {
                    Application.Quit();
                    return UpdateRes.Quit;
                }

                //版本号设回去
                if (YooAssetsMgr.Instance.Config.Resver != maxVer)
                {
                    return await ResetVersion(task);
                }
                return UpdateRes.Over;
            }

            
            //开始进行更新
            task.SetDownloadSize(size,0);

            //2、更新资源
            downloader.OnDownloadProgressCallback = (a, b, c, d) =>
            {
                task.SetDownloadSize(c,d);
            };
            downloader.BeginDownload();
            Log.Info("CheckResUpdate DownloadContent begin");
            await downloader.Task;

            if (downloader.Status != EOperationStatus.Succeed)
            {
                Log.Error(downloader.Error);
                return await UpdateFail(task, maxVer != YooAssetsMgr.Instance.Config.Resver);
            }
           
            op.SavePackageVersion();
            Log.Info("CheckResUpdate DownloadContent Success");
            return UpdateRes.Restart;
        }

        private async ETTask<UpdateRes> ProcessWithPackage(UpdateTask task)
        {
            var channel = YooAssetsMgr.Instance.CdnConfig.Channel;
            bool isAppMaxVer = !ServerConfigManager.Instance.FindMaxUpdateResVerThisAppVer(channel, task.AppVer, out var maxAppResVer);

            forceUpdate = Define.ForceUpdate;
            var verInfo = ServerConfigManager.Instance.GetResVerInfo(channel, YooAssetsMgr.Instance.Config.Resver);
            if (verInfo != null && verInfo.force_update == 1)
                forceUpdate = true;

            var maxVer = ServerConfigManager.Instance.FindMaxUpdateResVer(channel, "", maxAppResVer);
            if (maxVer < 0)
            {
                Log.Info("CheckResUpdate No Max Ver Channel = " + channel + " ");
                maxVer = YooAssetsMgr.Instance.Config.Resver;
            }

            if (isAppMaxVer)
            {
                maxVer = YooAssetsMgr.Instance.Config.Resver;
            }


            // 编辑器下跳过。
            // if (Define.IsEditor) return false;
            if (YooAssetsMgr.Instance.PlayMode != EPlayMode.HostPlayMode)
            {
                Log.Info("非网络运行模式");
                return UpdateRes.Over;
            }

            // 更新补丁清单
            Log.Info("更新补丁清单 " + maxVer);

            var op = YooAssetsMgr.Instance.UpdatePackageManifestAsync(maxVer.ToString(), false, task.TimeOut, null);
            await op.Task;
            if (op.Status != EOperationStatus.Succeed)
            {
                Log.Error(op.Error);
                return await UpdateFail(task, maxVer != YooAssetsMgr.Instance.Config.Resver);
            }

            var opList = new UpdatePackageManifestOperation[packageName.Length];
            for (int i = 0; i < packageName.Length; i++)
            {
                await YooAssetsMgr.Instance.GetPackage(packageName[i]);
                opList[i] = YooAssetsMgr.Instance.UpdatePackageManifestAsync(maxVer.ToString(), false, task.TimeOut,
                    packageName[i]);
                await opList[i].Task;
                if (opList[i].Status != EOperationStatus.Succeed)
                {
                    Log.Error(opList[i].Error);
                    return await UpdateFail(task, maxVer != YooAssetsMgr.Instance.Config.Resver);
                }
            }

            Log.Info("创建补丁下载器.");

            var downloaderList = new ResourceDownloaderOperation[packageName.Length + 1];
            downloaderList[0] =
                YooAssetsMgr.Instance.CreateResourceDownloader(task.DownloadingMaxNum, task.FailedTryAgain,
                    task.TimeOut, null);
            for (int i = 0; i < packageName.Length; i++)
            {
                downloaderList[i + 1] = YooAssetsMgr.Instance.CreateResourceDownloader(task.DownloadingMaxNum,
                    task.FailedTryAgain, task.TimeOut, packageName[i]);
            }

            using (ListComponent<ResourceDownloaderOperation> downloaderOperations =
                   ListComponent<ResourceDownloaderOperation>.Create())
            {
                //1.获取需要更新的大小
                long size = 0;
                for (int i = 0; i < downloaderList.Length; i++)
                {
                    if (downloaderList[i].TotalDownloadCount != 0)
                    {
                        if (downloaderList[i].TotalDownloadBytes > 0)
                        {
                            size += downloaderList[i].TotalDownloadBytes;
                            downloaderOperations.Add(downloaderList[i]);
                        }
                    }
                }

                if (size == 0)
                {
                    Log.Info("没有发现需要下载的资源");
                    return UpdateRes.Over;
                }

                //提示给用户
                Log.Info("downloadSize " + size);
                double sizeMb = size / (1024f * 1024f);
                Log.Info("CheckResUpdate res size_mb is " + sizeMb); //不屏蔽
                if (sizeMb > 0 && sizeMb < 0.01) sizeMb = 0.01;

                var ct = I18NManager.Instance.I18NGetParamText("Update_Info", sizeMb.ToString("0.00"));
                var btnState = await task.ShowMsgBoxView(ct, "Global_Btn_Confirm",
                    forceUpdate ? "Btn_Exit" : "Update_Skip");
                if (!btnState)
                {
                    if (forceUpdate)
                    {
                        Application.Quit();
                        return UpdateRes.Quit;
                    }

                    if (YooAssetsMgr.Instance.Config.Resver != maxVer)
                    {
                        return await ResetVersion(task);
                    }

                    return UpdateRes.Over;
                }

                //开始进行更新
                task.SetDownloadSize(size, 0);

                //2、更新资源
                long downloadSize = 0;
                Log.Info("CheckResUpdate DownloadContent begin");
                for (int i = 0; i < downloaderOperations.Count; i++)
                {
                    downloaderOperations[i].OnDownloadProgressCallback = (a, b, c, d) =>
                    {
                        task.SetDownloadSize(size, downloadSize + d);
                    };
                    downloaderOperations[i].BeginDownload();

                    await downloaderOperations[i].Task;

                    if (downloaderOperations[i].Status != EOperationStatus.Succeed)
                    {
                        Log.Error(downloaderOperations[i].Error);
                        return await UpdateFail(task, maxVer != YooAssetsMgr.Instance.Config.Resver);
                    }

                    downloadSize += downloaderOperations[i].TotalDownloadBytes;
                }


                op.SavePackageVersion();
                for (int i = 0; i < opList.Length; i++)
                {
                    opList[i].SavePackageVersion();
                }

                Log.Info("CheckResUpdate DownloadContent Success");
                return UpdateRes.Restart;

            }

        }
        /// <summary>
        /// 版本号设回去
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        private async ETTask<UpdateRes> ResetVersion(UpdateTask task)
        {
            var version = YooAssetsMgr.Instance.Config.Resver.ToString();
            var op = YooAssetsMgr.Instance
                .UpdatePackageManifestAsync(version, true, task.TimeOut, null);
            await op.Task;
            bool res = op.Status == EOperationStatus.Succeed;
            if (res && packageName != null)
            {
                for (int i = 0; i < packageName.Length; i++)
                {
                    op = YooAssetsMgr.Instance.UpdatePackageManifestAsync(version, false, task.TimeOut,
                        packageName[i]);
                    await op.Task;
                    if (op.Status != EOperationStatus.Succeed)
                    {
                        res = false;
                        break;
                    }
                }
            }
           
            if(!res)
            {
                Log.Error(op.Error);
                //设回去失败
                var btnState = await task.ShowMsgBoxView("Update_Get_Fail", "Update_ReTry", "Btn_Exit");
                if (btnState)
                {
                    return await Process(task);
                }
                else
                {
                    Application.Quit();
                    return UpdateRes.Quit;
                }
            }
            return UpdateRes.Over;
        }
        /// <summary>
        /// 下载失败
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        private async ETTask<UpdateRes> UpdateFail(UpdateTask task, bool reset)
        {
            var btnState = await task.ShowMsgBoxView("Update_Get_Fail", "Update_ReTry", this.forceUpdate?"Btn_Exit":"Update_Skip");
            if (btnState)
            {
                return await this.Process(task);
            }
            else if(this.forceUpdate)
            {
                Application.Quit();
                return UpdateRes.Quit;
            }

            if(reset) return await ResetVersion(task);
            return UpdateRes.Over;
        }
    }
}