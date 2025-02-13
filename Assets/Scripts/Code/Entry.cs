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
                ManagerProvider.RegisterManager<HttpManager>();
                
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
                ManagerProvider.RegisterManager<SceneManager>();
                
                ManagerProvider.RegisterManager<ServerConfigManager>();
                ManagerProvider.RegisterManager<NavmeshSystem>();
                
                ManagerProvider.RegisterManager<InputManager>();
                await StartGameAsync();
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }
        static async ETTask StartGameAsync()
        {
            if(YooAssetsMgr.Instance.PlayMode == EPlayMode.HostPlayMode && (Define.Networked||Define.ForceUpdate))
                await UIManager.Instance.OpenWindow<UIUpdateView,Action>(UIUpdateView.PrefabPath,StartGame);//下载热更资源
            else
                StartGame();
        }

        static void StartGame()
        {
            ManagerProvider.RegisterManager<SoundManager>();
            ManagerProvider.RegisterManager<BillboardSystem>();
            ManagerProvider.RegisterManager<ConfigSceneGroupCategory>();
            ManagerProvider.RegisterManager<ConfigAIDecisionTreeCategory>();
            ManagerProvider.RegisterManager<ConfigAbilityCategory>();
            ManagerProvider.RegisterManager<ConfigStoryCategory>();
            ManagerProvider.RegisterManager<CampManager>();
            SceneManager.Instance.SwitchScene<LoginScene>().Coroutine();
        }
    }
    
}