using UnityEngine;

namespace TaoTie
{
    public class SetUpdateListProcess: UpdateProcess
    {
        public override async ETTask<UpdateRes> Process(UpdateTask task)
        {
            var url = ServerConfigManager.Instance.GetUpdateListCdnUrl();
            // UpdateConfig aa = new UpdateConfig
            // {
            //     AppList = new()
            //     {
            //         {
            //             "googleplay",
            //             new AppConfig()
            //             {
            //                 AppUrl = "http://127.0.0.1",
            //                 AppVer = new()
            //                 {
            //                     {
            //                         1,
            //                         new Resver()
            //                         {
            //                             Channel = new() {"all"},
            //                             UpdateTailNumber = new() {"all"},
            //                         }
            //                     }
            //                 }
            //             }
            //         }
            //     },
            //     ResList = new()
            //     {
            //         {
            //             "googleplay",
            //             new()
            //             {
            //                 {
            //                     1,
            //                     new Resver
            //                     {
            //                         Channel = new() {"all"}, UpdateTailNumber = new() {"all"},
            //                     }
            //                 }
            //             }
            //         }
            //     }
            // };
            var info = await HttpManager.Instance.HttpGetResult<UpdateConfig>(url);
            if (info == null)
            {
                var btnState = await task.ShowMsgBoxView("Update_Get_Fail", "Update_ReTry", Define.ForceUpdate?"Btn_Exit":"Update_Skip");
                if (btnState)
                {
                    await this.Process(task);
                }
                else if(Define.ForceUpdate)
                {
                    Application.Quit();
                    return UpdateRes.Quit;
                }
            }
            else
            {
                ServerConfigManager.Instance.SetUpdateList(info);
            }
            return UpdateRes.Over;
        }
    }
}