using UnityEngine;
using YooAsset;

namespace TaoTie
{
    /// <summary>
    /// 需在SetUpdateListProcess后
    /// </summary>
    public class UpdateIsSHProcess: UpdateProcess
    {
        public override async ETTask<UpdateRes> Process(UpdateTask task)
        {
            await ETTask.CompletedTask;
            var channel = PackageManager.Instance.CdnConfig.Channel;
            int setVal = PlayerPrefs.GetInt("DEBUG_IsSH", 0);
            if (setVal == 0)
            {
                Define.IsSH = !ServerConfigManager.Instance.FindMaxUpdateResVerThisAppVer(channel, task.AppVer,out var maxAppResVer);
            }
            else
            {
                Define.IsSH = setVal == 1;
            }
            Log.Info("提审模式："+ Define.IsSH);
            return UpdateRes.Over;
        }

    }
}