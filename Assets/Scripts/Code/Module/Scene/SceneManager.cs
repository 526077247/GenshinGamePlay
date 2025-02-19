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
        public static SceneManager Instance{ get; private set; }
        
        //当前场景
        public IScene CurrentScene{ get; private set; }
        //是否忙
        public bool Busing { get; private set; } = false;
        
        private readonly Queue<ETTask> waitFinishTask = new Queue<ETTask>();
        #region override

        public void Init()
        {
            Instance = this;
        }

        public void Destroy()
        {
            waitFinishTask.Clear();
            Instance = null;
        }

        #endregion

        async ETTask<IScene> GetScene<T>() where T : class,IScene
        {
            var res = ObjectPool.Instance.Fetch<T>();
            await res.OnCreate();
            return res;
        }
        //切换场景
        async ETTask InnerSwitchScene<T>(bool needClean = false,int id = 0) where T:class,IScene
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
            await scene.OnEnter();
            await scene.SetProgress(slidValue);

            CameraManager.Instance.SetCameraStackAtLoadingStart();

            //等待资源管理器加载任务结束，否则很多Unity版本在切场景时会有异常，甚至在真机上crash
            Log.Info("InnerSwitchScene ProcessRunning Done ");
            while (ResourcesManager.Instance.IsProcessRunning())
            {
                await TimerManager.Instance.WaitAsync(1);
            }
            slidValue += 0.01f;
            await scene.SetProgress(slidValue);
            await TimerManager.Instance.WaitAsync(1);

            //清理UI
            Log.Info("InnerSwitchScene Clean UI");
            await UIManager.Instance.DestroyWindowExceptNames(scene.GetDontDestroyWindow());
            
            slidValue += 0.01f;
            await scene.SetProgress(slidValue);
            //清除ImageLoaderManager里的资源缓存 这里考虑到我们是单场景
            Log.Info("InnerSwitchScene ImageLoaderManager Cleanup");
            ImageLoaderManager.Instance.Clear();
            //清除预设以及其创建出来的gameObject, 这里不能清除loading的资源
            Log.Info("InnerSwitchScene GameObjectPool Cleanup");
            if (needClean && CurrentScene != null)
            {
                GameObjectPoolManager.GetInstance().Cleanup(true, CurrentScene.GetScenesChangeIgnoreClean());
                slidValue += 0.01f;
                await scene.SetProgress(slidValue);
                //清除除loading外的资源缓存 
                using (ListComponent<UnityEngine.Object> gos = ListComponent<UnityEngine.Object>.Create())
                {
                    for (int i = 0; i < CurrentScene.GetScenesChangeIgnoreClean().Count; i++)
                    {
                        var path = CurrentScene.GetScenesChangeIgnoreClean()[i];
                        var go = GameObjectPoolManager.GetInstance().GetCachedGoWithPath(path);
                        if (go != null)
                        {
                            gos.Add(go);
                        }
                    }
                    Log.Info("InnerSwitchScene ResourcesManager ClearAssetsCache excludeAssetLen = " + gos.Count);
                    ResourcesManager.Instance.ClearAssetsCache(gos);
                }
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
            await ResourcesManager.Instance.LoadSceneAsync(loadingScene.GetScenePath(), false);
            Log.Info("LoadSceneAsync Over");
            slidValue += 0.01f;
            await scene.SetProgress(slidValue);
            //GC：交替重复2次，清干净一点
            GC.Collect();
            GC.Collect();

            var res = Resources.UnloadUnusedAssets();
            while (!res.isDone)
            {
                await TimerManager.Instance.WaitAsync(1);
            }
            slidValue += 0.12f;
            await scene.SetProgress(slidValue);

            Log.Info("异步加载目标场景 Start");
            //异步加载目标场景
            await ResourcesManager.Instance.LoadSceneAsync(scene.GetScenePath(), false);
            await scene.OnComplete();
            slidValue += 0.65f;
            await scene.SetProgress(slidValue);
            //准备工作：预加载资源等
            await scene.OnPrepare();

            slidValue += 0.15f;
            await scene.SetProgress(slidValue);
            CameraManager.Instance.SetCameraStackAtLoadingDone();

            slidValue = 1;
            await scene.SetProgress(slidValue);
            Log.Info("等久点，跳的太快");
            //等久点，跳的太快
            await TimerManager.Instance.WaitAsync(500);
            Log.Info("加载目标场景完成 Start");
            CurrentScene = scene;
            await scene.OnSwitchSceneEnd();
            FinishLoad();
        }
        //切换场景
        public async ETTask SwitchScene<T>(bool needClean = false)where T:class,IScene
        {
            if (this.Busing) return;
            if (IsInTargetScene<T>())
                return;
            this.Busing = true;

            await this.InnerSwitchScene<T>(needClean);

            //释放loading界面引用的资源
            GameObjectPoolManager.GetInstance().CleanupWithPathArray(true, CurrentScene.GetScenesChangeIgnoreClean());
            this.Busing = false;
        }
        
        //切换场景
        public async ETTask SwitchMapScene(string typeName,Vector3 pos,Vector3 rot, bool needClean = false)
        {
            if (this.Busing) return;
            if(!SceneConfigCategory.Instance.TryGetByName(typeName,out var config)) return;
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
            this.Busing = true;

            await this.InnerSwitchScene<MapScene>(needClean, config.Id);
            avatar = (CurrentScene as MapScene)?.Self;
            if (avatar != null)
            {
                avatar.Position = pos;
                avatar.Rotation = Quaternion.Euler(rot);
            }
            //释放loading界面引用的资源
            GameObjectPoolManager.GetInstance().CleanupWithPathArray(true, CurrentScene.GetScenesChangeIgnoreClean());
            this.Busing = false;
        }

        public IScene GetCurrentScene()
        {
            return this.CurrentScene;
        }
        public T GetCurrentScene<T>() where T:IScene
        {
            return (T)this.CurrentScene;
        }
        public bool IsInTargetScene<T>() where T:IScene
        {
            if (this.CurrentScene == null) return false;
            return this.CurrentScene is T;
        }
        public bool IsInTargetMapScene(string name)
        {
            if (this.CurrentScene == null) return false;
            return this.CurrentScene.GetName() == name;
        }
        public ETTask WaitLoadOver()
        {
            ETTask task = ETTask.Create();
            waitFinishTask.Enqueue(task);
            return task;
        }
        
        public void FinishLoad()
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