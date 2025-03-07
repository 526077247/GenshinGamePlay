using UnityEngine;
using YooAsset;

namespace TaoTie
{
    public class AppUpdateProcess: UpdateProcess
    {
        public override async ETTask<UpdateRes> Process(UpdateTask task)
        {
            var appChannel = PackageManager.Instance.CdnConfig.Channel;
            var channelAppUpdateList = ServerConfigManager.Instance.GetAppUpdateListByChannel(appChannel);
            if (channelAppUpdateList == null || channelAppUpdateList.AppVer == null)
            {
                Log.Info("CheckAppUpdate channel_app_update_list or app_ver is nil, so return");
                return UpdateRes.Over;
            }
            var version = ServerConfigManager.Instance.FindMaxUpdateAppVer(appChannel);
            Log.Info("FindMaxUpdateAppVer =" + version);
            if (version<0)
            {
                Log.Info("CheckAppUpdate maxVer is nil");
                return UpdateRes.Over;
            }
    
            var appVer = task.AppVer;
            var flag = appVer - version;
            Log.Info(string.Format("CoCheckAppUpdate AppVer:{0} maxVer:{1}", appVer, version));
            if (flag >= 0)
            {
                Log.Info("CheckAppUpdate AppVer is Most Max Version, so return; flag = " + flag);
                return UpdateRes.Over;
            }

            var appURL = channelAppUpdateList.AppUrl;
            channelAppUpdateList.AppVer.TryGetValue(appVer, out var verInfo);//按当前版本来
            Log.Info("CheckAppUpdate app_url = " + appURL);
            if (!Define.ForceUpdate)
            {
                if (verInfo != null && verInfo.ForceUpdate == -1)//直接不提示
                    return UpdateRes.Over;
            }
            var forceUpdate = Define.ForceUpdate; 
            if (verInfo != null && verInfo.ForceUpdate != 0)
                forceUpdate = true;

            var check = CacheManager.Instance.GetInt(CacheKeys.CheckAppUpdate + version, 0);
            if (check != 0 && !forceUpdate)
            {
                return UpdateRes.Over;
            }

            var cancelBtnText = forceUpdate ? I18NKey.Btn_Exit : I18NKey.Btn_Enter_Game;
            var contentUpdate = forceUpdate ? I18NKey.Update_ReDownload : I18NKey.Update_SuDownload;
            var btnState = await task.ShowMsgBoxView(contentUpdate, I18NKey.Global_Btn_Confirm, cancelBtnText);

            if (btnState)
            {
                Application.OpenURL(appURL);
                //为了防止切换到网页后回来进入了游戏，所以这里需要继续进入该流程
                return await this.Process(task);
            }
            else if(forceUpdate)
            {
                Log.Info("CheckAppUpdate Need Force Update And User Choose Exit Game!");
                Application.Quit();
                return UpdateRes.Quit;
            }
            else
            {
                CacheManager.Instance.SetInt(CacheKeys.CheckAppUpdate + version, 1);
                CacheManager.Instance.Save();
            }
            // OfflineModeManager.Instance.SetOfflineMode(true);
            return UpdateRes.Over;
        }
    }
}