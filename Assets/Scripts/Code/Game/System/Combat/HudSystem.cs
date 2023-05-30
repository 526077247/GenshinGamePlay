using System.Collections;

namespace TaoTie
{
    public class HudSystem : IManager, IUpdate
    {
        public void Init()
        {
            
        }
        /// <summary>
        /// preload一些常用hud到pool
        /// </summary>
        /// <returns></returns>
        public async ETTask PreloadLoadAsset()
        {
            await ETTask.CompletedTask;
        }

        public void Destroy()
        {

        }

        public void Update()
        {
            var hudView = UIManager.Instance.GetWindow<UIDamageView>(1);
            if (hudView != null)
            {
                hudView.Update();
            }
        }
    }
}