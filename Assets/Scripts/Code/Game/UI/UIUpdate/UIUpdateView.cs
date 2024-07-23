using System;
namespace TaoTie
{
    public class UIUpdateView:UIBaseView,IOnCreate,IOnEnable<Action>,IOnWidthPaddingChange
    {
        public UISlider mSlider;
        
        public float lastProgress;
        public static string PrefabPath => "UI/UIUpdate/Prefabs/UIUpdateView.prefab";

        public Action OnOver;

        #region override

        public void OnCreate()
        {
            this.mSlider = this.AddComponent<UISlider>("Loadingscreen/Slider");
        }
        
        public void OnEnable(Action func)
        {
            this.OnOver = func;
            this.lastProgress = 0;
            this.mSlider.SetValue(0);
            this.StartCheckUpdate().Coroutine();
        }

        #endregion
        
        /// <summary>
        /// 设置进度
        /// </summary>
        /// <param name="value"></param>
        void SetProgress( float value)
        {
            if(value> this.lastProgress)
                this.lastProgress = value;
            this.mSlider.SetNormalizedValue(this.lastProgress);
        }
 
        /// <summary>
        /// 开始检测
        /// </summary>
        async ETTask StartCheckUpdate()
        {
            this.SetProgress(0);
            UpdateTask task = new UpdateTask();
            await task.Init(OnDownloadCallBack, 
                new SetWhiteListProcess(), 
                new SetUpdateListProcess(), 
                new AppUpdateProcess(),
                new MainPackageUpdateProcess());
           
            var res = await task.Process();
            if (res == UpdateRes.Restart)
            {
                Log.Info("更新完成，准备进入游戏");
                this.UpdateFinishAndStartGame().Coroutine();
            }
            else if (res == UpdateRes.Over)
            {
                Log.Info("不需要重启，直接进入游戏");
                this.OnOver?.Invoke();
            }
            else
            {
                Log.Error("UpdateTask fail");
            }
        }

        private void OnDownloadCallBack(long c ,long d)
        {
            float percent = (float) d / c;
            this.SetProgress(percent);
            // this.size.SetText($"{(d / (1024f * 1024f)).ToString("0.00")}MB/{(c / (1024f * 1024f)).ToString("0.00")}MB");
        }
        
        /// <summary>
        /// 更新完成
        /// </summary>
        private async ETTask UpdateFinishAndStartGame()
        {
            while (ResourcesManager.Instance.IsProcessRunning())
            {
                await TimerManager.Instance.WaitAsync(1);
            }
            await UIManager.Instance.DestroyAllWindow();
            GameObjectPoolManager.GetInstance().Cleanup();
            ResourcesManager.Instance.ClearAssetsCache();
            ObjectPool.Instance.Dispose();
            CodeLoader.Instance.ReStart();
        }

    }
}