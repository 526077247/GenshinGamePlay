﻿using System.Collections.Generic;
using UnityEngine;

namespace TaoTie
{
    public class MapScene:SceneManagerProvider,IScene
    {
        public int ConfigId;
        private SceneConfig config => SceneConfigCategory.Instance.Get(ConfigId);
        public override string GetName()
        {
            return config.Name;
        }

        public string GetScenePath()
        {
            return config.Perfab;
        }
        public void GetProgressPercent(out float cleanup, out float loadScene, out float prepare)
        {
            cleanup = 0.2f;
            loadScene = 0.65f;
            prepare = 0.15f;
        }
        
        #region 玩家信息
        /// <summary>
        /// 玩家unitId
        /// </summary>
        public long MyId;

        public Actor Self => GetManager<EntityManager>()?.Get<Actor>(MyId);

        #endregion
        private UILoadingView win;
        private string[] dontDestroyWindow = {TypeInfo<UILoadingView>.TypeName};

        public string[] GetDontDestroyWindow()
        {
            return dontDestroyWindow;
        }

        public List<string> GetScenesChangeIgnoreClean()
        {
            var res = new List<string>();
            res.Add(UILoadingView.PrefabPath); 
            return res;
        }
        public async ETTask OnCreate()
        {
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
            RemoveManager<StorySystem>();
            RemoveManager<SceneGroupManager>();
            RemoveManager<AIManager>();
            RemoveManager<EntityManager>();
            RemoveManager<ORCAManager>();
            RemoveManager<EnvironmentManager>();
            RemoveManager<GameTimerManager>();
        }

        public async ETTask OnPrepare(float progressStart,float progressEnd)
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
            var envm = RegisterManager<EnvironmentManager>();
            await envm.LoadAsync();
            if (config.DayNight == 1 && config.EnvIds.Length == 4)
            {
                envm.CreateDayNight(config.EnvIds[0], config.EnvIds[1], config.EnvIds[2], config.EnvIds[3]);
            }
            else if (config.DayNight == 0 && config.EnvIds.Length > 0)
            {
                envm.Create(config.EnvIds[0], EnvironmentPriorityType.Scene);
            }

            RegisterManager<ORCAManager>();
            var em = RegisterManager<EntityManager>();
            MyId = em.CreateEntity<Avatar, int>(1).Id;
            Self.GetComponent<EquipHoldComponent>().AddEquip(1).Coroutine();
            RegisterManager<AIManager, MapScene>(this);

            RegisterManager<SceneGroupManager, ulong[], SceneManagerProvider>(config.SceneGroupIds, this);
            RegisterManager<StorySystem, SceneManagerProvider>(this);
            await UIManager.Instance.OpenWindow<UIDamageView>(UIDamageView.PrefabPath, UILayerNames.GameLayer);
            await UIManager.Instance.OpenWindow<UIOpView>(UIOpView.PrefabPath, UILayerNames.GameLayer);
            if (PlatformUtil.IsMobile())
                await UIManager.Instance.OpenWindow<UIMobileMainView>(UIMobileMainView.PrefabPath);
            var model = Self.GetComponent<UnitModelComponent>();
            await model.WaitLoadGameObjectOver();
            await UIManager.Instance.DestroyWindow<UILoadingView>();
            win = null;
            Log.Info("进入场景 " + GetScenePath());
        }
    }
}