using System.Collections.Generic;

namespace TaoTie
{
    public class LoadingScene:IScene
    {
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