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
                await UIManager.Instance.OpenWindow<UILoadingView>(UILoadingView.PrefabPath);
                
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
            using ListComponent<ETTask> tasks0 = ListComponent<ETTask>.Create();
            tasks0.Add(InitSDK());
            if (!configInit) tasks0.Add(ConfigManager.Instance.LoadAsync());
            await ETTaskHelper.WaitAll(tasks0);

            ManagerProvider.RegisterManager<PerformanceManager>();
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
            using ListComponent<ETTask> tasks = ListComponent<ETTask>.Create();
            tasks.Add(ConfigFsmControllerCategory.Instance.LoadAsync());
            tasks.Add(ConfigAIBetaCategory.Instance.LoadAsync());
            tasks.Add(ConfigActorCategory.Instance.LoadAsync());
            tasks.Add(SoundManager.Instance.InitAsync());
            tasks.Add(InputManager.Instance.LoadAsync());
            tasks.Add(CameraManager.Instance.LoadAsync());
            tasks.Add(ConfigSceneGroupCategory.Instance.LoadAsync());
            tasks.Add(ConfigAIDecisionTreeCategory.Instance.LoadAsync());
            tasks.Add(ConfigAbilityCategory.Instance.LoadAsync());
            tasks.Add(ConfigStoryCategory.Instance.LoadAsync());
            tasks.Add(GameObjectPoolManager.GetInstance().PreLoadGameObjectAsync(UIToast.PrefabPath, 1));
            await ETTaskHelper.WaitAll(tasks);

            await PackageManager.Instance.UnloadUnusedAssets(Define.DefaultName);
            ManagerProvider.RegisterManager<CampManager>();
            SceneManager.Instance.SwitchScene<LoginScene>().Coroutine();
        }
        
        static async ETTask InitSDK()
		{
			ETTask task = ETTask.Create(true);
#if MINIGAME_SUBPLATFORM_DOUYIN
			//build setting 添加包依赖
			TTSDK.TT.InitSDK((code, env) =>
			{
				task.SetResult();
				Log.Info("TT.InitSDK " + code);
			});
#elif MINIGAME_SUBPLATFORM_WEIXIN
#if UNITY_EDITOR
			task.SetResult();
#else
			//build setting 添加包依赖
			WeChatWASM.WX.InitSDK((code) =>
			{
				task.SetResult();
				Log.Info("WX.InitSDK " + code);
			});
#endif
#elif MINIGAME_SUBPLATFORM_KUAISHOU
#if UNITY_EDITOR
			task.SetResult();
#else
			//build setting 添加包依赖
			KSWASM.KS.InitSDK((code) =>
			{
				task.SetResult();
				Log.Info("KS.InitSDK " + code);
			});
#endif
#elif MINIGAME_SUBPLATFORM_MINIHOST
#if UNITY_EDITOR
			task.SetResult();
#else
			//build setting 添加包依赖
			minihost.TJ.InitSDK((code) =>
			{
				task.SetResult();
				Log.Info("minihost.InitSDK " + code);
			});
#endif
#else
			task.SetResult();
#endif
			await task;
		}
    }
    
}