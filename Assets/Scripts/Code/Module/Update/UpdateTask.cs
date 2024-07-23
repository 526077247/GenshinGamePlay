using System;
using UnityEngine;

namespace TaoTie
{
    public class UpdateTask
    {
        public int AppVer { get; private set; }
        private UpdateProcess[] list;
        private Action<long, long> onDownloadSize;
        public int DownloadingMaxNum = 10;
        public int FailedTryAgain = 2;
        public int TimeOut = 8;
        
        public async ETTask Init(Action<long,long> downloadSizeCallBack,  params UpdateProcess[] process)
        {
            onDownloadSize = downloadSizeCallBack;
            list = process;
            AppVer = int.Parse(Application.version.Split(".")[2]);
            await GameObjectPoolManager.GetInstance().PreLoadGameObjectAsync(UIMsgBoxWin.PrefabPath,1);
        }

        public async ETTask<UpdateRes> Process()
        {
            if (list == null)
            {
                Log.Error("UpdateTask 未 Init");
                return UpdateRes.Fail;
            }

            for (int i = 0; i < list.Length; i++)
            {
                if (list[i] == null) continue;
                var res = await list[i].Process(this);
                switch (res)
                {
                    case UpdateRes.Fail:
                        Log.Error("Update Fail "+list[i].GetType().Name);
                        return UpdateRes.Fail;
                    case UpdateRes.Over:
                        break;
                    case UpdateRes.Quit:
                        Application.Quit();
                        return UpdateRes.Quit;
                    case UpdateRes.Restart:
                        return UpdateRes.Restart;
                }
            }

            return UpdateRes.Over;
        }


        public void SetDownloadSize(long totalDownloadBytes, long currentDownloadBytes)
        {
            onDownloadSize?.Invoke(totalDownloadBytes,currentDownloadBytes);
        }


        #region MsgBox

        private MsgBoxPara para = new MsgBoxPara();

        /// <summary>
        /// 提示窗
        /// </summary>
        /// <param name="content"></param>
        /// <param name="confirmBtnText"></param>
        /// <param name="cancelBtnText"></param>
        /// <returns></returns>
        public async ETTask<bool> ShowMsgBoxView(string content, string confirmBtnText, string cancelBtnText)
        {
            ETTask<bool> tcs = ETTask<bool>.Create();
            void ConfirmBtnFunc()
            { 
                tcs.SetResult(true);
            }
            void CancelBtnFunc()
            {
                tcs.SetResult(false);
            }

            I18NManager.Instance.I18NTryGetText(content, out this.para.Content);
            I18NManager.Instance.I18NTryGetText(confirmBtnText, out this.para.ConfirmText);
            I18NManager.Instance.I18NTryGetText(cancelBtnText, out this.para.CancelText);
            this.para.ConfirmCallback = ConfirmBtnFunc;
            this.para.CancelCallback = CancelBtnFunc;
            await UIManager.Instance.OpenWindow<UIMsgBoxWin, MsgBoxPara>(UIMsgBoxWin.PrefabPath,
                this.para,UILayerNames.TipLayer);
            var result = await tcs;
            await UIManager.Instance.CloseWindow<UIMsgBoxWin>();
            return result;
        }
        

        #endregion
    }
}