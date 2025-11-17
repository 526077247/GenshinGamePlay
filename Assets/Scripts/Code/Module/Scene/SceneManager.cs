using System;
using System.Collections.Generic;
using UnityEngine;
using YooAsset;

namespace TaoTie
{
    /// <summary>
    /// 场景管理系统：调度和控制场景异步加载以及进度管理，展示loading界面和更新进度条数据，GC、卸载未使用资源等
    /// 注意：
    /// 资源预加载放各个场景类中自行控制
    /// </summary>
    public class SceneManager : IManager
    {
        public static SceneManager Instance { get; private set; }

        /// <summary>
        /// 当前场景
        /// </summary>
        public IScene CurrentScene { get; private set; }

        private SceneHandle currentSceneOp;
        /// <summary>
        /// 是否正在加载
        /// </summary>
        public bool Busing { get; private set; } = false;

        private readonly Queue<ETTask> waitFinishTask = new Queue<ETTask>();

        #region override

        public void Init()
        {
            Instance = this;
        }

        public void Destroy()
        {
            currentSceneOp?.UnloadAsync();
            currentSceneOp = null;
            waitFinishTask.Clear();
            Instance = null;
        }

        #endregion

        async ETTask<IScene> GetScene<T>() where T : class, IScene
        {
            var res = ObjectPool.Instance.Fetch<T>();
            await res.OnCreate();
            return res;
        }
        
        async ETTask InnerSwitchScene<T>(bool needClean = false, int id = 0, List<string> ignoreClean = null) where T : class, IScene
        {
            float slidValue = 0;
            Log.Info("InnerSwitchScene start open uiLoading");
            var scene = await GetScene<T>();
            if (CurrentScene != null)
            {
                await CurrentScene.OnLeave();
                ObjectPool.Instance.Recycle(CurrentScene);
            }

            if (scene is MapScene map)
            {
                map.ConfigId = id;
            }

            scene.GetProgressPercent(out float cleanup, out float loadScene, out float prepare);
            float total = cleanup + loadScene + prepare;
            cleanup = cleanup /total * 0.9f;
            loadScene =  loadScene /total * 0.9f;
            prepare = prepare /total * 0.9f;

            await scene.OnEnter();
            await scene.SetProgress(slidValue);

            CameraManager.Instance.SetCameraStackAtLoadingStart();

            //等待资源管理器加载任务结束，否则很多Unity版本在切场景时会有异常，甚至在真机上crash
            Log.Info("InnerSwitchScene ProcessRunning Done ");
            while (ResourcesManager.Instance.IsProcessRunning() && !ResourcesManager.Instance.IsPreloadScene())
            {
                await TimerManager.Instance.WaitAsync(1);
            }

            slidValue += 0.01f;
            await scene.SetProgress(slidValue);
            await TimerManager.Instance.WaitAsync(1);

            //清理UI
            Log.Info("InnerSwitchScene Clean UI");
            await UIManager.Instance.DestroyWindowExceptNames(scene.GetDontDestroyWindow());
            await UIManager.Instance.DestroyAllBox();
            slidValue += 0.01f;
            await scene.SetProgress(slidValue);
            //清除ImageLoaderManager里的资源缓存 这里考虑到我们是单场景
            Log.Info("InnerSwitchScene ImageLoaderManager Cleanup");
            ImageLoaderManager.Instance.Cleanup();
            //清除预设以及其创建出来的gameObject, 这里不能清除loading的资源
            Log.Info("InnerSwitchScene GameObjectPool Cleanup");
            if (needClean)
            {
                GameObjectPoolManager.GetInstance().Cleanup(true, ignoreClean);
                slidValue += 0.01f;
                await scene.SetProgress(slidValue);
                await PackageManager.Instance.UnloadUnusedAssets(Define.DefaultName);
                slidValue += 0.01f;
                await scene.SetProgress(slidValue);
            }
            else
            {
                slidValue += 0.02f;
                await scene.SetProgress(slidValue);
            }

            var loadingScene = await GetScene<LoadingScene>();
            var loadingOp = await ResourcesManager.Instance.LoadSceneAsync(loadingScene.GetScenePath(), false);
            currentSceneOp?.UnloadAsync();
            currentSceneOp = loadingOp;
            
            Log.Info("LoadingScene Over");
            slidValue += 0.01f;
            await scene.SetProgress(slidValue);
            //GC：交替重复2次，清干净一点
            GC.Collect();
            GC.Collect();

            var res = Resources.UnloadUnusedAssets();
            while (!res.isDone && !ResourcesManager.Instance.IsPreloadScene())
            {
                await TimerManager.Instance.WaitAsync(1);
            }

            slidValue += cleanup;
            await scene.SetProgress(slidValue);

            Log.Info("异步加载目标场景");
            //异步加载目标场景
            var op = await ResourcesManager.Instance.LoadSceneAsync(scene.GetScenePath(), false);
            currentSceneOp?.UnloadAsync();
            currentSceneOp = op;
            CameraManager.Instance.SetCameraStackAtLoadingDone();
            await scene.OnComplete();
            slidValue += loadScene;
            await scene.SetProgress(slidValue);
            //准备工作：预加载资源等
            await scene.OnPrepare(slidValue, slidValue + prepare);

            slidValue += prepare;
            await scene.SetProgress(slidValue);

            slidValue = 1;
            await scene.SetProgress(slidValue);
            Log.Info("等久点，跳的太快");
            //等久点，跳的太快
            await TimerManager.Instance.WaitAsync(500);
            Log.Info("加载目标场景完成");
            CurrentScene = scene;
            await scene.OnSwitchSceneEnd();
            FinishLoad();
        }

        /// <summary>
        /// 预加载场景。中途不能加载其他资源，不能取消，只能等场景加载完成
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public async ETTask PreloadScene<T>() where T : class, IScene
        {
            var scene = await GetScene<T>();
            ResourcesManager.Instance.PreLoadScene(scene.GetScenePath(), false);
        }

        /// <summary>
        /// 预加载场景。中途不能加载其他资源，不能取消，只能等场景加载完成
        /// </summary>
        /// <param name="typeName"></param>
        public void PreloadMapScene(string typeName)
        {
            if(!SceneConfigCategory.Instance.TryGetByName(typeName,out var config)) return;
            ResourcesManager.Instance.PreLoadScene(config.Perfab, false);
        }
        /// <summary>
        /// 切换场景
        /// </summary>
        /// <param name="needClean"></param>
        /// <typeparam name="T"></typeparam>
        public async ETTask SwitchScene<T>(bool needClean = true) where T : class, IScene
        {
            if (Busing) return;
            if (IsInTargetScene<T>())
                return;
            Busing = true;
            var ignoreClean = CurrentScene?.GetScenesChangeIgnoreClean();
            await InnerSwitchScene<T>(needClean, ignoreClean:ignoreClean);

            //释放loading界面引用的资源
            GameObjectPoolManager.GetInstance().CleanupWithPathArray(ignoreClean);
            Busing = false;
        }

        /// <summary>
        /// 切换Map场景
        /// </summary>
        /// <param name="typeName"></param>
        /// <param name="pos"></param>
        /// <param name="rot"></param>
        /// <param name="needClean"></param>
        public async ETTask SwitchMapScene(string typeName, Vector3 pos, Vector3 rot, bool needClean = true)
        {
            if (Busing) return;
            if (!SceneConfigCategory.Instance.TryGetByName(typeName, out var config)) return;
            Unit avatar;
            if (IsInTargetMapScene(config.Name))
            {
                avatar = (CurrentScene as MapScene)?.Self;
                if (avatar != null)
                {
                    avatar.Position = pos;
                    avatar.Rotation = Quaternion.Euler(rot);
                }

                return;
            }

            Busing = true;
            var ignoreClean = CurrentScene?.GetScenesChangeIgnoreClean();
            await InnerSwitchScene<MapScene>(needClean, config.Id, ignoreClean);
            avatar = (CurrentScene as MapScene)?.Self;
            if (avatar != null)
            {
                avatar.Position = pos;
                avatar.Rotation = Quaternion.Euler(rot);
            }

            //释放loading界面引用的资源
            GameObjectPoolManager.GetInstance().CleanupWithPathArray(ignoreClean);
            Busing = false;
        }

        public T GetCurrentScene<T>() where T : IScene
        {
            return (T) CurrentScene;
        }

        public bool IsInTargetScene<T>() where T : IScene
        {
            if (CurrentScene == null) return false;
            return CurrentScene is T;
        }

        public bool IsInTargetMapScene(string name)
        {
            if (CurrentScene == null) return false;
            return CurrentScene.GetName() == name;
        }

        public ETTask WaitLoadOver()
        {
            ETTask task = ETTask.Create();
            waitFinishTask.Enqueue(task);
            return task;
        }

        private void FinishLoad()
        {
            int count = waitFinishTask.Count;
            while (count-- > 0)
            {
                ETTask task = waitFinishTask.Dequeue();
                task.SetResult();
            }
        }

    }
}