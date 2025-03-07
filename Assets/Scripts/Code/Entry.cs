using System;
using System.Threading;
using YooAsset;

namespace TaoTie
{
    public static class Entry
    {
        public static void Start()
        {
            StartAsync().Coroutine();
        }

        private static async ETTask StartAsync()
        {
            try
            {
                ManagerProvider.RegisterManager<Messager>();
                ManagerProvider.RegisterManager<LogManager>();
                
                ManagerProvider.RegisterManager<AttributeManager>();
                
                ManagerProvider.RegisterManager<CoroutineLockManager>();
                ManagerProvider.RegisterManager<TimerManager>();
                
                ManagerProvider.RegisterManager<CacheManager>();

                var cm = ManagerProvider.RegisterManager<ConfigManager>();
                await cm.LoadAsync();
                
                ManagerProvider.RegisterManager<ResourcesManager>();
                ManagerProvider.RegisterManager<GameObjectPoolManager>();
                ManagerProvider.RegisterManager<ImageLoaderManager>();
                ManagerProvider.RegisterManager<MaterialManager>();
                
                ManagerProvider.RegisterManager<I18NManager>();
                ManagerProvider.RegisterManager<UIManager>();
                ManagerProvider.RegisterManager<UIMsgBoxManager>();
                ManagerProvider.RegisterManager<UIToastManager>();

                ManagerProvider.RegisterManager<CameraManager>();
                await CameraManager.Instance.LoadAsync();
                ManagerProvider.RegisterManager<SceneManager>();
                
                ManagerProvider.RegisterManager<ServerConfigManager>();
                ManagerProvider.RegisterManager<NavmeshSystem>();
                
                ManagerProvider.RegisterManager<InputManager>();
                if(PackageManager.Instance.PlayMode == EPlayMode.HostPlayMode && (Define.Networked||Define.ForceUpdate))
                    await UIManager.Instance.OpenWindow<UIUpdateView,Action>(UIUpdateView.PrefabPath,StartGame);//下载热更资源
                else
                    StartGame();
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }
        static void StartGame()
        {
            StartGameAsync().Coroutine();
        }

        static async ETTask StartGameAsync()
        {
            ManagerProvider.RegisterManager<SoundManager>();
            ManagerProvider.RegisterManager<BillboardSystem>();
            ManagerProvider.RegisterManager<ConfigSceneGroupCategory>();
            ManagerProvider.RegisterManager<ConfigAIDecisionTreeCategory>();
            ManagerProvider.RegisterManager<ConfigAbilityCategory>();
            ManagerProvider.RegisterManager<ConfigStoryCategory>();
            ManagerProvider.RegisterManager<ConfigFsmControllerCategory>();
            ManagerProvider.RegisterManager<ConfigAIBetaCategory>();
            ManagerProvider.RegisterManager<ConfigActorCategory>();
            using (ListComponent<ETTask> tasks = ListComponent<ETTask>.Create())
            {
                tasks.Add(ConfigSceneGroupCategory.Instance.LoadAsync());
                tasks.Add(ConfigAIDecisionTreeCategory.Instance.LoadAsync());
                tasks.Add(ConfigAbilityCategory.Instance.LoadAsync());
                tasks.Add(ConfigStoryCategory.Instance.LoadAsync());
                tasks.Add(ConfigFsmControllerCategory.Instance.LoadAsync());
                tasks.Add(ConfigAIBetaCategory.Instance.LoadAsync());
                tasks.Add(ConfigActorCategory.Instance.LoadAsync());
                await ETTaskHelper.WaitAll(tasks);
            }
            await PackageManager.Instance.UnloadUnusedAssets(Define.DefaultName);
            ManagerProvider.RegisterManager<CampManager>();
            SceneManager.Instance.SwitchScene<LoginScene>().Coroutine();
        }
    }
    
}