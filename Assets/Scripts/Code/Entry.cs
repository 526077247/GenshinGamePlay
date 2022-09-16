using System;
using System.Threading;

namespace TaoTie
{
    public static class Entry
    {
        public static void Start()
        {
            try
            {
                ManagerProvider.GetManager<AttributeManager>();
                
                ManagerProvider.GetManager<CoroutineLockManager>();
                ManagerProvider.GetManager<TimerManager>();
                
                ManagerProvider.GetManager<ConfigManager>();
                ManagerProvider.GetManager<ResourcesManager>();
                ManagerProvider.GetManager<GameObjectPoolManager>();
                ManagerProvider.GetManager<ImageLoaderManager>();
                ManagerProvider.GetManager<MaterialManager>();
                
                ManagerProvider.GetManager<I18NManager>();
                ManagerProvider.GetManager<UIManager>();
                ManagerProvider.GetManager<UILayersManager>();

                ManagerProvider.GetManager<CameraManager>();
                ManagerProvider.GetManager<SceneManager>();
                
                ManagerProvider.GetManager<ServerConfigManager>();
                StartGameAsync().Coroutine();
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }
        static async ETTask StartGameAsync()
        {
            if(Define.Networked||Define.ForceUpdate)
                await UIManager.Instance.OpenWindow<UIUpdateView,Action>(UIUpdateView.PrefabPath,StartGame);//下载热更资源
            else
                StartGame();
        }

        static void StartGame()
        {
            SceneManager.Instance.SwitchScene<LoginScene>().Coroutine();
        }
    }
    
}