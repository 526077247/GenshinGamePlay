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
            //     app_list = new Dictionary<string, AppConfig>
            //     {
            //         {
            //             "googleplay",
            //             new AppConfig()
            //             {
            //                 app_url = "http://127.0.0.1",
            //                 app_ver = new Dictionary<int, Resver>()
            //                 {
            //                     {
            //                         1,
            //                         new Resver()
            //                         {
            //                             channel = new List<string>() { "all" },
            //                             update_tailnumber = new List<string>() { "all" },
            //                         }
            //                     }
            //                 }
            //             }
            //         }
            //     },
            //     res_list = new Dictionary<string, Dictionary<int, Resver>>
            //     {
            //         {
            //             "googleplay",
            //             new Dictionary<int, Resver>
            //             {
            //                 {
            //                     1,
            //                     new Resver
            //                     {
            //                         channel = new List<string>() { "all" }, update_tailnumber = new List<string>() { "all" },
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