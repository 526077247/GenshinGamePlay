using System.Collections.Generic;
using UnityEngine;
using System;
using YooAsset;
namespace TaoTie
{
    public class UIUpdateView:UIBaseView,IOnCreate,IOnEnable<Action>
    {
        public readonly int BTN_CANCEL = 1;
        public readonly int BTN_CONFIRM = 2;

        public UISlider m_slider;

        public MsgBoxPara para { get; private set; } = new MsgBoxPara();
        
        public float last_progress;
        public static string PrefabPath => "UI/UIUpdate/Prefabs/UIUpdateView.prefab";

        public Action OnOver;
        public bool force_update;

        public YooAsset.PatchDownloaderOperation Downloader;
        public int StaticVersion;

        #region override

        public void OnCreate()
        {
            this.m_slider = this.AddComponent<UISlider>("Loadingscreen/Slider");
        }
        
        public void OnEnable(Action func)
        {
            this.force_update = Define.ForceUpdate;
            this.OnOver = func;
            this.last_progress = 0;
            this.m_slider.SetValue(0);
            this.StartCheckUpdate().Coroutine();
        }

        #endregion
        
        /// <summary>
        /// 设置进度
        /// </summary>
        /// <param name="value"></param>
        void SetProgress( float value)
        {
            if(value> this.last_progress)
                this.last_progress = value;
            this.m_slider.SetNormalizedValue(this.last_progress);
        }
        /// <summary>
        /// 提示窗
        /// </summary>
        /// <param name="content"></param>
        /// <param name="confirmBtnText"></param>
        /// <param name="cancelBtnText"></param>
        /// <returns></returns>
        async ETTask<int> ShowMsgBoxView(string content, string confirmBtnText, string cancelBtnText)
        {
            ETTask<int> tcs = ETTask<int>.Create();
            Action confirmBtnFunc = () =>
             {
                 tcs.SetResult(this.BTN_CONFIRM);
             };

            Action cancelBtnFunc = () =>
            {
                tcs.SetResult(this.BTN_CANCEL);
            };
            I18NManager.Instance.I18NTryGetText(content, out this.para.Content);
            I18NManager.Instance.I18NTryGetText(confirmBtnText, out this.para.ConfirmText);
            I18NManager.Instance.I18NTryGetText(cancelBtnText, out this.para.CancelText);
            this.para.ConfirmCallback = confirmBtnFunc;
            this.para.CancelCallback = cancelBtnFunc;
            await UIManager.Instance.OpenWindow<UIMsgBoxWin, MsgBoxPara>(UIMsgBoxWin.PrefabPath,
                this.para,UILayerNames.TipLayer);
            var result = await tcs;
            await UIManager.Instance.CloseWindow<UIMsgBoxWin>();
            return result;
        }
        /// <summary>
        /// 开始检测
        /// </summary>
        /// <param name="this"></param>
        public async ETTask StartCheckUpdate()
        {
            await this.CheckIsInWhiteList();

            await this.CheckUpdateList();

            var Over = await this.CheckAppUpdate();
            if (Over) return;
            
            var isUpdateDone = await this.CheckResUpdate();
            if (isUpdateDone)
            {
                Log.Info("更新完成，准备进入游戏");
                this.UpdateFinishAndStartGame().Coroutine();
            }
            else
            {
                Log.Info("不需要更新，直接进入游戏");
                this.OnOver?.Invoke();
                await this.CloseSelf();
            }
        }

        #region 更新流程
        
        /// <summary>
        /// 白名单
        /// </summary>
        /// <param name="this"></param>
        async ETTask CheckIsInWhiteList()
        {
            var url = ServerConfigManager.Instance.GetWhiteListCdnUrl();
            if (string.IsNullOrEmpty(url))
            {
                Log.Info(" no white list cdn url");
                return;
            }
            var info = await HttpManager.Instance.HttpGetResult<List<WhiteConfig>>(url);
            if (info != null)
            {
                ServerConfigManager.Instance.SetWhiteList(info);
                if (ServerConfigManager.Instance.IsInWhiteList())
                {
                    var btnState = await this.ShowMsgBoxView("Update_White", "Global_Btn_Confirm", "Global_Btn_Cancel");
                    if (btnState == this.BTN_CONFIRM)
                    {
                        ServerConfigManager.Instance.SetWhiteMode(true);
                    }
                }
                return;
            }
        }

        /// <summary>
        /// 版本号信息
        /// </summary>
        /// <param name="this"></param>
        async ETTask CheckUpdateList()
        {
            var url = ServerConfigManager.Instance.GetUpdateListCdnUrl();
            //UpdateConfig aa = new UpdateConfig
            //{
            //    app_list = new Dictionary<string, AppConfig>
            //    {
                    
            //    },
            //    res_list = new Dictionary<string, Dictionary<string, Resver>>
            //    {
            //        {"100",new Dictionary<string, Resver>{
            //            { "1",new Resver{
            //                channel = new List<string>(){"all"},
            //                update_tailnumber = new List<string>(){"all"},
            //            } }
            //        }}
            //    }
            //};
            var info = await HttpManager.Instance.HttpGetResult<UpdateConfig>(url);
            if (info == null)
            {
                var btnState = await this.ShowMsgBoxView("Update_Get_Fail", "Update_ReTry", this.force_update?"Btn_Exit":"Update_Skip");
                if (btnState == this.BTN_CONFIRM)
                {
                    await this.CheckUpdateList();
                }
                else if(this.force_update)
                {
                    Application.Quit();
                    return;
                }
            }
            else
            {
                ServerConfigManager.Instance.SetUpdateList(info);
            }
        }

        /// <summary>
        /// 是否需要整包更新
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        async ETTask<bool> CheckAppUpdate()
        {
            var app_channel = PlatformUtil.GetAppChannel();
            var channel_app_update_list = ServerConfigManager.Instance.GetAppUpdateListByChannel(app_channel);
            if (channel_app_update_list == null || channel_app_update_list.app_ver == null)
            {
                Log.Info("CheckAppUpdate channel_app_update_list or app_ver is nil, so return");
                return false;
            }
            this.StaticVersion = ServerConfigManager.Instance.FindMaxUpdateAppVer(app_channel);
            Log.Info("FindMaxUpdateAppVer =" + this.StaticVersion);
            if (this.StaticVersion<0)
            {
                Log.Info("CheckAppUpdate maxVer is nil");
                return false;
            }
            int app_ver = int.Parse(Application.version);
            var flag = app_ver - this.StaticVersion;
            Log.Info(string.Format("CoCheckAppUpdate AppVer:{0} maxVer:{1}", app_ver, this.StaticVersion));
            if (flag >= 0)
            {
                Log.Info("CheckAppUpdate AppVer is Most Max Version, so return; flag = " + flag);
                return false;
            }

            var app_url = channel_app_update_list.app_url;
            var verInfo = channel_app_update_list.app_ver[app_ver];
            Log.Info("CheckAppUpdate app_url = " + app_url);

            this.force_update = Define.ForceUpdate; 
            if (Define.ForceUpdate)//默认强更
            {
                if (verInfo != null && verInfo.force_update == 0)
                    this.force_update = false;
            }
            else
            {
                if (verInfo != null && verInfo.force_update != 0)
                    this.force_update = true;
            }


            var cancelBtnText = this.force_update ? "Btn_Exit" : "Btn_Enter_Game";
            var content_updata = this.force_update ? "Update_ReDownload" : "Update_SuDownload";
            var btnState = await this.ShowMsgBoxView(content_updata, "Global_Btn_Confirm", cancelBtnText);

            if (btnState == this.BTN_CONFIRM)
            {
                Application.OpenURL(app_url);
                //为了防止切换到网页后回来进入了游戏，所以这里需要继续进入该流程
                return await this.CheckAppUpdate();
            }
            else if(this.force_update)
            {
                Log.Info("CheckAppUpdate Need Force Update And User Choose Exit Game!");
                Application.Quit();
                return true;
            }
            return false;
        }
        
        /// <summary>
        /// 资源更新检查，并根据版本来修改资源cdn地址
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public async ETTask<bool> CheckResUpdate()
        {
            var app_channel = PlatformUtil.GetAppChannel();
            var channel = YooAssetsMgr.Instance.Config.Channel;
            this.StaticVersion = ServerConfigManager.Instance.FindMaxUpdateResVer(channel, app_channel,out var verInfo);
            if (this.StaticVersion<0)
            {
                Log.Info("CheckResUpdate No Max Ver Channel = " + channel + " app_channel " + app_channel);
                return false;
            }
            this.force_update = Define.ForceUpdate; 
            if (!Define.ForceUpdate)//默认强更
            {
                if (verInfo != null && verInfo.force_update != 0)
                    this.force_update = true;
            }
            // if (this.StaticVersion>= maxVer)
            // {
            //     Log.Info("CheckResUpdate ResVer is Most Max Version, so return;");
            //     return false;
            // }

            // 编辑器下跳过。
            // if (Define.IsEditor) return false;
            if (YooAssets.PlayMode != YooAssets.EPlayMode.HostPlayMode)
            {
                Log.Info("非网络运行模式");
                return false;
            }
            ETTask task = ETTask.Create(true);
            // 更新补丁清单
            Log.Info("更新补丁清单");
            var operation = YooAssets.UpdateManifestAsync(this.StaticVersion, 30);
            operation.Completed += (op) =>
            {
                task.SetResult();
            };
            await task;
            int btnState;
            if(operation.Status != EOperationStatus.Succeed)
            {
                btnState = await this.ShowMsgBoxView("Update_Get_Fail", "Update_ReTry", this.force_update?"Btn_Exit":"Update_Skip");
                if (btnState == this.BTN_CONFIRM)
                {
                    return await this.CheckResUpdate();
                }
                else if(this.force_update)
                {
                    Application.Quit();
                    return false;
                }
            }

            Log.Info("创建补丁下载器.");
            int downloadingMaxNum = 10;
            int failedTryAgain = 3;
            this.Downloader = YooAssets.CreatePatchDownloader(downloadingMaxNum, failedTryAgain);
            if (this.Downloader.TotalDownloadCount == 0)
            {
                Log.Info("没有发现需要下载的资源");
                return false;
            }
            
            //获取需要更新的大小
            var size = this.Downloader.TotalDownloadBytes;
            //提示给用户
            Log.Info("downloadSize " + size);
            double size_mb = size / (1024f * 1024f);
            Log.Info("CheckResUpdate res size_mb is " + size_mb);//不屏蔽
            if (size_mb > 0 && size_mb < 0.01) size_mb = 0.01;

            var ct = I18NManager.Instance.I18NGetParamText("Update_Info",size_mb.ToString("0.00"));
            btnState = await this.ShowMsgBoxView(ct, "Global_Btn_Confirm", this.force_update?"Btn_Exit":"Update_Skip");
            if (btnState == this.BTN_CANCEL)
            {
                if (this.force_update)
                {
                    Application.Quit();
                    return false;
                }
                return true;
            }

            //开始进行更新

            this.last_progress = 0;
            this.SetProgress(0);
            //2、更新资源
            ETTask<bool> downloadTask = ETTask<bool>.Create(true);
            this.Downloader.OnDownloadOverCallback += (a)=>{downloadTask.SetResult(a);};
            this.Downloader.OnDownloadProgressCallback =(a,b,c,d)=>
            {
                this.SetProgress((float)d/c);
            };
            this.Downloader.BeginDownload();
            Log.Info("CheckResUpdate DownloadContent begin");
            bool result = await downloadTask;
            if (!result) return false;
            Log.Info("CheckResUpdate DownloadContent Success");
            return true;
        }
        
        /// <summary>
        /// 更新完成
        /// </summary>
        /// <param name="this"></param>
        private async ETTask UpdateFinishAndStartGame()
        {
            PlayerPrefs.SetInt("STATIC_VERSION",this.StaticVersion);
            PlayerPrefs.Save();
            while (ResourcesManager.Instance.IsProsessRunning())
            {
                await TimerManager.Instance.WaitAsync(1);
            }
            ResourcesManager.Instance.ClearAssetsCache();
            ManagerProvider.Clear();
            YooAssetsMgr.Instance.ClearConfigCache();
            //热修复
            // AddressablesManager.Instance.StartInjectFix();
            CodeLoader.Instance.ReStart();
        }
        #endregion

    }
}