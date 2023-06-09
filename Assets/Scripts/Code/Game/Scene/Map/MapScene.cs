using System.Collections.Generic;
using UnityEngine;

namespace TaoTie
{
    public class MapScene:SceneManagerProvider,IScene
    {
        public int ConfigId;
        private SceneConfig config => SceneConfigCategory.Instance.Get(ConfigId);
        public string GetName()
        {
            return config.Name;
        }

        public string GetScenePath()
        {
            return config.Perfab;
        }
        
        #region 玩家信息
        /// <summary>
        /// 玩家unitId
        /// </summary>
        public long MyId;

        public Actor Self => GetManager<EntityManager>().Get<Actor>(MyId);

        #endregion
        private UILoadingView win;
        private string[] dontDestroyWindow = {"UILoadingView"};
        private List<string> scenesChangeIgnoreClean;
        
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
            RemoveManager<SceneGroupManager>();
            RemoveManager<AIManager>();
            RemoveManager<EntityManager>();
            RemoveManager<GameTimerManager>();
        }

        public async ETTask OnPrepare()
        {
            await BillboardSystem.Instance.PreloadLoadAsset();
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

        public virtual async ETTask OnSwitchSceneEnd()
        {
            RegisterManager<GameTimerManager>();
            var em = RegisterManager<EntityManager>();
            MyId = em.CreateEntity<Avatar, int>(1).Id;
            Self.GetComponent<EquipHoldComponent>().AddEquip(1).Coroutine();
            RegisterManager<AIManager,MapScene>(this);

            RegisterManager<SceneGroupManager,List<ConfigSceneGroup>,SceneManagerProvider>(ConfigSceneGroupCategory.Instance.GetAllList(),this);
            
            await UIManager.Instance.OpenWindow<UIDamageView>(UIDamageView.PrefabPath,UILayerNames.GameLayer);
            await UIManager.Instance.OpenWindow<UIOpView>(UIOpView.PrefabPath, UILayerNames.GameLayer);
            var selfGhc = Self.GetComponent<GameObjectHolderComponent>();
            await selfGhc.WaitLoadGameObjectOver();
            await UIManager.Instance.DestroyWindow<UILoadingView>();
            win = null;
            Log.Info("进入场景 " + GetScenePath());
        }
    }
}