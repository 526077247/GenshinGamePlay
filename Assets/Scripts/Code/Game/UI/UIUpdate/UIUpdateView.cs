using System;

namespace TaoTie
{
    public class UIUpdateView : UIBaseView, IOnCreate, IOnEnable<Action>, IOnWidthPaddingChange
    {
        public static string PrefabPath => "UI/UIUpdate/Prefabs/UIUpdateView.prefab";
        private UISlider mSlider;
        private float lastProgress;
        private Action onOver;

        #region override

        public void OnCreate()
        {
            mSlider = AddComponent<UISlider>("Loadingscreen/Slider");
        }

        public void OnEnable(Action func)
        {
            onOver = func;
            lastProgress = 0;
            mSlider.SetValue(0);
            StartCheckUpdate().Coroutine();
        }

        #endregion

        /// <summary>
        /// 设置进度
        /// </summary>
        /// <param name="value"></param>
        void SetProgress(float value)
        {
            if (value > lastProgress || value == 0)
                lastProgress = value;
            mSlider.SetNormalizedValue(lastProgress);
        }

        /// <summary>
        /// 开始检测
        /// </summary>
        async ETTask StartCheckUpdate()
        {
            SetProgress(0);
            UpdateTask task = new UpdateTask();
            using (ListComponent<string> names = ListComponent<string>.Create())
            {
                if (PackageManager.Instance.Config.OtherPackageMaxVer != null)
                {
                    foreach (var item in PackageManager.Instance.Config.OtherPackageMaxVer)
                    {
                        if (item.Value != null) names.AddRange(item.Value);
                    }
                }

                await task.Init(OnDownloadCallBack,
                    new SetWhiteListProcess(),
                    new SetUpdateListProcess(),
                    new UpdateIsSHProcess(),
                    new AppUpdateProcess(),
                    new MainPackageUpdateProcess(),
                    new OtherPackageUpdateProcess(names.ToArray()));
            }

            var res = await task.Process();
            if (res == UpdateRes.Restart)
            {
                Log.Info("更新完成，准备进入游戏");
                UpdateFinishAndStartGame().Coroutine();
            }
            else if (res == UpdateRes.Over)
            {
                Log.Info("不需要重启，直接进入游戏");
                onOver?.Invoke();
            }
            else
            {
                Log.Error("UpdateTask fail");
            }
        }

        private void OnDownloadCallBack(long c, long d)
        {
            float percent = (float) d / c;
            SetProgress(percent);
            // size.SetText($"{(d / (1024f * 1024f)).ToString("0.00")}MB/{(c / (1024f * 1024f)).ToString("0.00")}MB");
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
            await PackageManager.Instance.UnloadUnusedAssets(Define.DefaultName);
            ObjectPool.Instance.Dispose();
            CodeLoader.Instance.ReStart();
        }

    }
}