using System.Collections.Generic;

namespace TaoTie
{
    public class CampManager: IManager
    {
        public static CampManager Instance { get; private set; }

        private UnOrderMultiMapSet<uint, uint> alliances;
        #region IManager

        public void Init()
        {
            Instance = this;
            alliances = new UnOrderMultiMapSet<uint, uint>();
        }

        public void Destroy()
        {
            alliances.Clear();
            Instance = null;
        }

        #endregion

        /// <summary>
        /// 注册同盟
        /// </summary>
        /// <param name="one"></param>
        /// <param name="other"></param>
        private void RegisterAlliances(uint one, uint other)
        {
            alliances.Add(one, other);
            alliances.Add(other, one);
        }

        /// <summary>
        /// 是否为同盟
        /// </summary>
        /// <param name="self"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool IsAlliances(uint self, uint other)
        {
            return alliances.Contains(self, other);
        }
    }
}