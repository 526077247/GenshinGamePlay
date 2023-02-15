using System.Collections.Generic;
using UnityEngine;

namespace TaoTie
{
    public abstract class BaseMapScene:SceneManagerProvider,IScene
    {
        public abstract string Name { get; }
        #region 玩家信息
        /// <summary>
        /// 玩家unitId
        /// </summary>
        public long MyId;

        public Unit Self => GetManager<EntityManager>().Get<Unit>(MyId);

        #endregion
        private UILoadingView win;
        private string[] dontDestroyWindow = {"UILoadingView"};
        public List<string> scenesChangeIgnoreClean;
        
        public string[] GetDontDestroyWindow()
        {
            return dontDestroyWindow;
        }

        public List<string> GetScenesChangeIgnoreClean()
        {
            return scenesChangeIgnoreClean;
        }
        public async ETTask OnCreate()
        {
            scenesChangeIgnoreClean = new List<string>();
            scenesChangeIgnoreClean.Add(UILoadingView.PrefabPath);
            await ETTask.CompletedTask;
        }

        public async ETTask OnEnter()
        {
            win = await UIManager.Instance.OpenWindow<UILoadingView>(UILoadingView.PrefabPath);
            win.SetProgress(0);
        }

        public async ETTask OnLeave()
        {
            await ETTask.CompletedTask;
            RemoveManager<EntityManager>();
            // RemoveManager<InputManager>();
            // RemoveManager<NpcManager>();
        }

        public async ETTask OnPrepare()
        {
            await ETTask.CompletedTask;
        }

        public async ETTask OnComplete()
        {
            await ETTask.CompletedTask;
        }

        public async ETTask SetProgress(float value)
        {
            win.SetProgress(value);
            await ETTask.CompletedTask;
        }

        public abstract string GetScenePath();

        public virtual async ETTask OnSwitchSceneEnd()
        {
            RegisterManager<GameTimerManager>();
            RegisterManager<AIManager>();
            // await UIManager.Instance.OpenWindow<UIHudView>(UIHudView.PrefabPath);
            // await UIManager.Instance.OpenWindow<UIMainView>(UIMainView.PrefabPath);
            var em = RegisterManager<EntityManager>();
            MyId = em.CreateEntity<Avatar, int>(1).Id;
            
            RegisterManager<GearManager,List<ConfigGear>,SceneManagerProvider>(ConfigGearCategory.Instance.GetAllList(),this);
            // RegisterManager<InputManager,BaseMapScene>(this);
            await UIManager.Instance.DestroyWindow<UILoadingView>();
            win = null;
            Log.Info("进入场景 " + GetScenePath());
        }
    }
}