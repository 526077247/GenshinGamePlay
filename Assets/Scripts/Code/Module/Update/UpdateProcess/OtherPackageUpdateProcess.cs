using System;
using UnityEngine;
using YooAsset;

namespace TaoTie
{
    public class OtherPackageUpdateProcess: UpdateProcess
    {
        public string[] PackageNames;
        public Func<ETTask<bool>> OnUpdateFail;
        public bool ForceUpdate = false;
        private BuildInConfig Config => PackageManager.Instance.Config;
        public override async ETTask<UpdateRes> Process(UpdateTask task)
        {
            Log.Info("ProcessWithPackage");
            // 非网络运行模式下跳过。
            if (PackageManager.Instance.PlayMode == EPlayMode.WebPlayMode && 
                PackageManager.Instance.PlayMode == EPlayMode.HostPlayMode)
            {
                Log.Info("非网络运行模式");
                return UpdateRes.Over;
            }
            if (PackageNames == null || PackageNames.Length <= 0) return UpdateRes.Over;
            
            // 更新补丁清单
            Log.Info("更新补丁清单");
            for (int i = 0; i < PackageNames.Length; i++)
            {
                await PackageManager.Instance.GetPackage(PackageNames[i]);
                var maxVer = Config.GetPackageMaxVersion(PackageNames[i]);
                var version = PackageManager.Instance.GetPackageVersion(PackageNames[i]);
                if (version != maxVer)
                {
                    var op = PackageManager.Instance.UpdatePackageManifestAsync(maxVer.ToString(), task.TimeOut, null);
                    await op.Task;
                    if(op.Status!= EOperationStatus.Succeed)
                    {
                        Log.Error(op.Error);
                        return await UpdateFail(task, maxVer != version);
                    }
                }
            }
            
            Log.Info("创建补丁下载器.");
            ResourceDownloaderOperation downloader = null;
            for (int i = 0; i < PackageNames.Length; i++)
            {
                var temp = PackageManager.Instance.CreateResourceDownloader(task.DownloadingMaxNum, task.FailedTryAgain, task.TimeOut, PackageNames[i]);
                if (temp.TotalDownloadCount != 0)
                {
                    if (downloader == null)
                    {
                        downloader = temp;
                    }
                    else
                    {
                        downloader.Combine(temp);
                    }
                }
            }

            if (downloader == null)
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
                ForceUpdate ? "Btn_Exit" : "Update_Skip");
            if (!btnState)
            {
                if (ForceUpdate)
                {
                    Application.Quit();
                    return UpdateRes.Quit;
                }

                //版本号设回去
                return await ResetVersion(task);
            }

            
            //开始进行更新
            task.SetDownloadSize(size,0);

            //2、更新资源
            downloader.DownloadUpdateCallback = (a) =>
            {
                task.SetDownloadSize(a.TotalDownloadBytes,a.CurrentDownloadBytes);
            };
            downloader.BeginDownload();
            Log.Info("CheckResUpdate DownloadContent begin");
            await downloader.Task;

            if (downloader.Status != EOperationStatus.Succeed)
            {
                Log.Error(downloader.Error);
                return await UpdateFail(task, true);
            }
            
            Log.Info("CheckResUpdate DownloadContent Success");
            return UpdateRes.Restart;
            
        }

        /// <summary>
        /// 下载失败
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        private async ETTask<UpdateRes> UpdateFail(UpdateTask task, bool reset)
        {
            bool btnState;
            if (OnUpdateFail != null)
            {
                btnState = await OnUpdateFail();
            }
            else
            {
                btnState = await task.ShowMsgBoxView("Update_Get_Fail", "Update_ReTry",
                    this.ForceUpdate ? "Btn_Exit" : "Update_Skip");
            } 
            if (btnState)
            {
                return await this.Process(task);
            }
            else if(this.ForceUpdate)
            {
                Application.Quit();
                return UpdateRes.Quit;
            }

            if(reset) return await ResetVersion(task);
            return UpdateRes.Over;
        }
        
        /// <summary>
        /// 版本号设回去
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        private async ETTask<UpdateRes> ResetVersion(UpdateTask task)
        {
            bool res = true;
            for (int i = 0; i < PackageNames.Length; i++)
            {
                var version = PackageManager.Instance.GetPackageVersion(PackageNames[i]);
                if (version < 0) continue;
                var op = PackageManager.Instance
                    .UpdatePackageManifestAsync(version.ToString(), task.TimeOut, null);
                await op.Task;
                if (op.Status != EOperationStatus.Succeed)
                {
                    res &= op.Status == EOperationStatus.Succeed;
                }
                else
                {
                    Log.Error(op.Error);
                }
                if (!res)
                {
                    break;
                }
            }

            if(!res)
            {
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
    }
}