using System.Collections.Generic;

namespace TaoTie
{
    public class LoginScene:IScene
    {
        private UILoadingView win;
        private string[] dontDestroyWindow = {"UILoadingView"};

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
        public void GetProgressPercent(out float cleanup, out float loadScene, out float prepare)
        {
            cleanup = 0.2f;
            loadScene = 0.65f;
            prepare = 0.15f;
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