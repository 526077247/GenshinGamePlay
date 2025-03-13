using System.Collections.Generic;

namespace TaoTie
{
    public class LoadingScene:IScene
    {
        public void GetProgressPercent(out float cleanup, out float loadScene, out float prepare)
        {
            cleanup = 0.2f;
            loadScene = 0.65f;
            prepare = 0.15f;
        }

        public string GetName()
        {
            return "Loading";
        }
        public string[] GetDontDestroyWindow()
        {
            return null;
        }

        public List<string> GetScenesChangeIgnoreClean()
        {
            return null;
        }
        public async ETTask OnCreate()
        {
            await ETTask.CompletedTask;
        }

        public async ETTask OnEnter()
        {
            await ETTask.CompletedTask;
        }

        public async ETTask OnLeave()
        {
            await ETTask.CompletedTask;
        }

        public async ETTask OnPrepare(float progressStart,float progressEnd)
        {
            await ETTask.CompletedTask;
        }

        public async ETTask OnComplete()
        {
            await ETTask.CompletedTask;
        }

        public async ETTask SetProgress(float value)
        {
            await ETTask.CompletedTask;
        }

        public string GetScenePath()
        {
            return "Scenes/LoadingScene/Loading.unity";
        }

        public async ETTask OnSwitchSceneEnd()
        {
            await ETTask.CompletedTask;
        }

        public void Dispose()
        {
            
        }
    }
}