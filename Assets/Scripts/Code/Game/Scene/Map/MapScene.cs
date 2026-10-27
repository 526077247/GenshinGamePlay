using System.Collections.Generic;
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
        private NavmeshTerrainQuery navmeshTerrainQuery;

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
            TerrainQueryManager.Instance?.Unregister(navmeshTerrainQuery);
            navmeshTerrainQuery = null;
            RemoveManager<StorySystem>();
            RemoveManager<SceneGroupManager>();
            RemoveManager<AIManager>();
            RemoveManager<EntityManager>();
            RemoveManager<ORCASystem>();
            RemoveManager<ActorBodySystem>();
            RemoveManager<EnvironmentManager>();
            RemoveManager<GameTimerManager>();
        }

        public async ETTask OnPrepare(float progressStart,float progressEnd)
        {
            await BillboardSystem.Instance.PreloadLoadAsset();
            await SetupTerrainQuery();
        }

        /// <summary>
        /// 预加载本场景 NavMesh 数据并注册地形/水面查询。
        /// NavMesh 数据是同步查询的前提（<see cref="NavmeshSystem.TryGetNearestPoly"/> 只读已加载的查询对象），
        /// 因此必须在创建 Actor（出生点求解）之前完成。
        /// </summary>
        private async ETTask SetupTerrainQuery()
        {
            TerrainQueryManager terrain = TerrainQueryManager.Instance;
            if (terrain == null)
            {
                Log.Error("地形查询管理器未注册，无法初始化场景地形数据");
                return;
            }

            string navMeshName = config.NavMesh;
            if (string.IsNullOrEmpty(navMeshName))
            {
                // 该场景无 NavMesh 数据，出生点退化为纯物理搜索。
                return;
            }

            if (NavmeshSystem.Instance == null)
            {
                Log.Error("寻路系统未注册，无法加载场景 NavMesh 数据");
                return;
            }

            if (!await NavmeshSystem.Instance.Preload(navMeshName))
            {
                Log.Error($"场景 NavMesh 数据加载失败: {navMeshName}");
                return;
            }

            navmeshTerrainQuery = new NavmeshTerrainQuery(navMeshName);
            terrain.Register(navmeshTerrainQuery);
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

            RegisterManager<ORCASystem>();
            RegisterManager<ActorBodySystem>();
            var em = RegisterManager<EntityManager>();
            MyId = em.CreateEntity<Avatar, int>(1).Id;
            Self.GetComponent<EquipHoldComponent>().AddEquip(1).Coroutine();
            RegisterManager<AIManager>().SetLocalAvatar(Self);

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