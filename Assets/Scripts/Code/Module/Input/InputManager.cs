using System.Collections.Generic;

namespace TaoTie
{
    public class InputManager: IManager, IUpdateManager
    {
        public static InputManager Instance { get; private set; }
        public bool IsPause;
        /// <summary>
        /// 按键绑定
        /// </summary>
        private readonly Dictionary<int, int> keySetMap = new Dictionary<int, int>();
        #region IManager

        public void Init()
        {
            Instance = this;
        }

        public void Destroy()
        {
            Instance = null;
        }

        #endregion

        public void Update()
        {
            if(IsPause) return;
        }
    }
}