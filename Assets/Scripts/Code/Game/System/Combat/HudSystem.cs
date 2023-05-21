using System.Collections;

namespace TaoTie
{
    public class HudSystem : IManager, IUpdate
    {
        public void Init()
        {
            PreloadLoadAsset().Coroutine();
        }
        /// <summary>
        /// preload一些常用hud到pool
        /// </summary>
        /// <returns></returns>
        private async ETTask PreloadLoadAsset()
        {
            await ETTask.CompletedTask;
        }

        public void Destroy()
        {

        }

        public void Update()
        {
            var hudView = UIManager.Instance.GetWindow<UIHudView>(1);
            if (hudView != null)
            {
                hudView.Update();
            }
        }
    }
}