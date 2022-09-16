using System;
using System.Collections.Generic;
using UnityEngine;

namespace TaoTie
{
    /// <summary>
    /// 场景管理系统：调度和控制场景异步加载以及进度管理，展示loading界面和更新进度条数据，GC、卸载未使用资源等
    /// 注意：
    /// 资源预加载放各个场景类中自行控制
    /// </summary>
    public class SceneManager:IManager
    {
        public static SceneManager Instance;

        public Dictionary<Type, IScene> Scenes;
        //当前场景
        public IScene CurrentScene;
        //是否忙
        public bool Busing = false;
        
        
        #region override

        public void Init()
        {
            this.Scenes = new Dictionary<Type, IScene>();
            Instance = this;
        }

        public void Destroy()
        {
            this.Scenes = null;
            Instance = null;
        }

        #endregion

        async ETTask<IScene> GetScene<T>() where T : IScene
        {
            if (Scenes.TryGetValue(typeof(T), out var res))
            {
                return res;
            }
            res = Activator.CreateInstance<T>();
            await res.OnCreate();
            Scenes.Add(typeof(T),res);
            return res;
        }

        //切换场景
        async ETTask InnerSwitchScene<T>(bool needclean = false)where T:IScene
        {
            float slid_value = 0;
            Log.Info("InnerSwitchScene start open uiloading");
            var scene = await GetScene<T>();
            if(CurrentScene!=null)
                await CurrentScene.OnLeave();
            await scene.OnEnter();
            await scene.SetProgress(slid_value);

            CameraManager.Instance.SetCameraStackAtLoadingStart();

            //等待资源管理器加载任务结束，否则很多Unity版本在切场景时会有异常，甚至在真机上crash
            Log.Info("InnerSwitchScene ProsessRunning Done ");
            while (ResourcesManager.Instance.IsProsessRunning())
            {
                await TimerManager.Instance.WaitAsync(1);
            }
            slid_value += 0.01f;
            await scene.SetProgress(slid_value);
            await TimerManager.Instance.WaitAsync(1);

            //清理UI
            Log.Info("InnerSwitchScene Clean UI");
            await UIManager.Instance.DestroyWindowExceptNames(scene.GetDontDestroyWindow());
            
            slid_value += 0.01f;
            await scene.SetProgress(slid_value);
            //清除ImageLoaderManager里的资源缓存 这里考虑到我们是单场景
            Log.Info("InnerSwitchScene ImageLoaderManager Cleanup");
            ImageLoaderManager.Instance.Clear();
            //清除预设以及其创建出来的gameobject, 这里不能清除loading的资源
            Log.Info("InnerSwitchScene GameObjectPool Cleanup");
            if (needclean)
            {
                GameObjectPoolManager.Instance.Cleanup(true, CurrentScene.GetScenesChangeIgnoreClean());
                slid_value += 0.01f;
                await scene.SetProgress(slid_value);
                //清除除loading外的资源缓存 
                List<UnityEngine.Object> gos = new List<UnityEngine.Object>();
                for (int i = 0; i < CurrentScene.GetScenesChangeIgnoreClean().Count; i++)
                {
                    var path = CurrentScene.GetScenesChangeIgnoreClean()[i];
                    var go = GameObjectPoolManager.Instance.GetCachedGoWithPath(path);
                    if (go != null)
                    {
                        gos.Add(go);
                    }
                }
                Log.Info("InnerSwitchScene ResourcesManager ClearAssetsCache excludeAssetLen = " + gos.Count);
                ResourcesManager.Instance.ClearAssetsCache(gos.ToArray());
                slid_value += 0.01f;
                await scene.SetProgress(slid_value);
            }
            else
            {
                slid_value += 0.02f;
                await scene.SetProgress(slid_value);
            }

            var loadingScene = await GetScene<LoadingScene>();
            await ResourcesManager.Instance.LoadSceneAsync(loadingScene.GetScenePath(), false);
            Log.Info("LoadSceneAsync Over");
            slid_value += 0.01f;
            await scene.SetProgress(slid_value);
            //GC：交替重复2次，清干净一点
            GC.Collect();
            GC.Collect();

            var res = Resources.UnloadUnusedAssets();
            while (!res.isDone)
            {
                await TimerManager.Instance.WaitAsync(1);
            }
            slid_value += 0.12f;
            await scene.SetProgress(slid_value);

            Log.Info("异步加载目标场景 Start");
            //异步加载目标场景
            await ResourcesManager.Instance.LoadSceneAsync(scene.GetScenePath(), false);
            await scene.OnComplete();
            slid_value += 0.65f;
            await scene.SetProgress(slid_value);
            //准备工作：预加载资源等
            await scene.OnPrepare();

            slid_value += 0.15f;
            await scene.SetProgress(slid_value);
            CameraManager.Instance.SetCameraStackAtLoadingDone();

            slid_value = 1;
            await scene.SetProgress(slid_value);
            //等久点，跳的太快
            await TimerManager.Instance.WaitAsync(500);
            await scene.OnSwitchSceneEnd();
            CurrentScene = scene;
        }
        //切换场景
        public async ETTask SwitchScene<T>(bool needclean = false)where T:IScene
        {
            if (this.Busing) return;
            if (IsInTargetScene<T>())
                return;
            this.Busing = true;

            await this.InnerSwitchScene<T>(needclean);

            //释放loading界面引用的资源
            GameObjectPoolManager.Instance.CleanupWithPathArray(true, CurrentScene.GetScenesChangeIgnoreClean());
            this.Busing = false;
        }

        public IScene GetCurrentScene()
        {
            return this.CurrentScene;
        }

        public bool IsInTargetScene<T>()where T:IScene
        {
            if (this.CurrentScene == null) return false;
            return this.CurrentScene.GetType() == typeof(T);
        }
        
    }
}