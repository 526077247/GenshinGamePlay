using System.Collections.Generic;

namespace TaoTie
{
    public class SetWhiteListProcess: UpdateProcess
    {
        public override async ETTask<UpdateRes> Process(UpdateTask task)
        {
            var url = ServerConfigManager.Instance.GetWhiteListCdnUrl();
            if (string.IsNullOrEmpty(url))
            {
                Log.Info(" no white list cdn url");
                return UpdateRes.Over;
            }

            var info = await HttpManager.Instance.HttpGetResult<List<WhiteConfig>>(url);
            if (info != null)
            {
                ServerConfigManager.Instance.SetWhiteList(info);
                if (ServerConfigManager.Instance.IsInWhiteList())
                {
                    var btnState = await task.ShowMsgBoxView("Update_White", "Global_Btn_Confirm",
                        "Global_Btn_Cancel");
                    if (btnState)
                    {
                        ServerConfigManager.Instance.SetWhiteMode(true);
                    }
                }

                return UpdateRes.Over;
            }

            return UpdateRes.Over;
        }
    }
}