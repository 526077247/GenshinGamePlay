using System.Collections.Generic;

namespace TaoTie
{
    public class LoginScene:IScene
    {
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
            scenesChangeIgnoreClean .Clear();
            scenesChangeIgnoreClean = null;
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
            win.SetProgress(value);
            await ETTask.CompletedTask;
        }
        public string GetName()
        {
            return "Login";
        }
        public string GetScenePath()
        {
            return "Scenes/LoginScene/Login.unity";
        }

        public async ETTask OnSwitchSceneEnd()
        {
            await UIManager.Instance.OpenWindow<UILobbyView>(UILobbyView.PrefabPath);
            await UIManager.Instance.DestroyWindow<UILoadingView>();
            win = null;
        }
    }
}