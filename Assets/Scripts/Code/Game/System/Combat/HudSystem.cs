using System.Collections;

namespace TaoTie
{
    public class HudSystem : IManager, IUpdateManager
    {
        public void Init()
        {
            //PreloadLoadAsset().Coroutine();
        }
        /// <summary>
        /// preloadһЩ����hud��pool
        /// </summary>
        /// <returns></returns>
        private async ETTask PreloadLoadAsset()
        {

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