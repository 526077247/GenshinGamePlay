using System;
using System.Threading;
using Obfuz;
using YooAsset;

namespace TaoTie
{
    [ObfuzIgnore]
    public static class Entry
    {
        [ObfuzIgnore]
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

                ManagerProvider.RegisterManager<ConfigManager>();
                
                ManagerProvider.RegisterManager<ResourcesManager>();
                ManagerProvider.RegisterManager<GameObjectPoolManager>();
                
                ManagerProvider.RegisterManager<I18NManager>();
                ManagerProvider.RegisterManager<UIManager>();
                
                if(PackageManager.Instance.PlayMode == EPlayMode.HostPlayMode && (Define.Networked||Define.ForceUpdate))
                {
                    await ConfigManager.Instance.LoadAsync();
                    ManagerProvider.RegisterManager<ServerConfigManager>();
                    await UIManager.Instance.OpenWindow<UIUpdateView, Action>(UIUpdateView.PrefabPath,
                        UpdateOverStartGame); //下载热更资源
                }
                else
                {
                    await StartGameAsync(false);
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }
        static void UpdateOverStartGame()
        {
            StartGameAsync(true).Coroutine();
        }

        static async ETTask StartGameAsync(bool configInit)
        {
            ManagerProvider.RegisterManager<ImageLoaderManager>();
            ManagerProvider.RegisterManager<MaterialManager>();
            ManagerProvider.RegisterManager<SceneManager>();
            ManagerProvider.RegisterManager<CameraManager>();
            ManagerProvider.RegisterManager<InputManager>();
            ManagerProvider.RegisterManager<NavmeshSystem>();
            ManagerProvider.RegisterManager<SoundManager>();
            ManagerProvider.RegisterManager<AbilitySystem>();
            ManagerProvider.RegisterManager<ModelSystem>();
            ManagerProvider.RegisterManager<BillboardSystem>();
            ManagerProvider.RegisterManager<FsmSystem>();
            ManagerProvider.RegisterManager<MoveSystem>();
            
            ManagerProvider.RegisterManager<ConfigSceneGroupCategory>();
            ManagerProvider.RegisterManager<ConfigAIDecisionTreeCategory>();
            ManagerProvider.RegisterManager<ConfigAbilityCategory>();
            ManagerProvider.RegisterManager<ConfigStoryCategory>();
            ManagerProvider.RegisterManager<ConfigFsmControllerCategory>();
            ManagerProvider.RegisterManager<ConfigAIBetaCategory>();
            ManagerProvider.RegisterManager<ConfigActorCategory>();
            
            GameObjectPoolManager.GetInstance().AddPersistentPrefabPath(UIToast.PrefabPath);
            using (ListComponent<ETTask> tasks = ListComponent<ETTask>.Create())
            {
                if (!configInit)
                {
                    tasks.Add(ConfigManager.Instance.LoadAsync());
                }
                else
                {
                    tasks.Add(ConfigFsmControllerCategory.Instance.LoadAsync());
                    tasks.Add(ConfigAIBetaCategory.Instance.LoadAsync());
                    tasks.Add(ConfigActorCategory.Instance.LoadAsync());
                }
                tasks.Add(SoundManager.Instance.InitAsync());
                tasks.Add(InputManager.Instance.LoadAsync());
                tasks.Add(CameraManager.Instance.LoadAsync());
                tasks.Add(ConfigSceneGroupCategory.Instance.LoadAsync());
                tasks.Add(ConfigAIDecisionTreeCategory.Instance.LoadAsync());
                tasks.Add(ConfigAbilityCategory.Instance.LoadAsync());
                tasks.Add(ConfigStoryCategory.Instance.LoadAsync());
                tasks.Add(GameObjectPoolManager.GetInstance().PreLoadGameObjectAsync(UIToast.PrefabPath, 1));
                await ETTaskHelper.WaitAll(tasks);
            }
            if (!configInit)
            {
                using (ListComponent<ETTask> tasks = ListComponent<ETTask>.Create())
                {
                    tasks.Add(ConfigFsmControllerCategory.Instance.LoadAsync());
                    tasks.Add(ConfigAIBetaCategory.Instance.LoadAsync());
                    tasks.Add(ConfigActorCategory.Instance.LoadAsync());
                    await ETTaskHelper.WaitAll(tasks);
                }
            }
            await PackageManager.Instance.UnloadUnusedAssets(Define.DefaultName);
            ManagerProvider.RegisterManager<CampManager>();
            SceneManager.Instance.SwitchScene<LoginScene>().Coroutine();
        }
    }
    
}