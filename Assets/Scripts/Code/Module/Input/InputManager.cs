using System;
using System.Collections.Generic;
using UnityEngine;

namespace TaoTie
{
    public class InputManager : IManager, IUpdateManager
    {
        public static InputManager Instance { get; private set; }
        public bool IsPause;

        /// <summary>
        /// 按键绑定
        /// </summary>
        private readonly Dictionary<int, int> keySetMap = new Dictionary<int, int>();

        /// <summary>
        /// 按键状态
        /// </summary>
        private readonly Dictionary<int, bool> keyStatus = new Dictionary<int, bool>();

        #region IManager

        public void Init()
        {
            Instance = this;
        }

        public void Destroy()
        {
            Instance = null;
            keySetMap.Clear();
            keyStatus.Clear();
        }

        #endregion

        public void Update()
        {
            if (IsPause) return;
            foreach (var keyValue in keySetMap)
            {
                var keyCode = (KeyCode)keyValue.Value;
                keyStatus[keyValue.Value] = Input.GetKey(keyCode);
            }
        }

        /// <summary>
        ///  获取按键是否按下
        /// </summary>
        /// <param name="keyCode"></param>
        /// <returns></returns>
        public bool InputGetKey(int keyCode)
        {
            if (keyStatus.ContainsKey(keyCode))
            {
                return keyStatus[keyCode];
            }

            Log.Error($"keyCode Fail! keyCode = {keyCode}");
            return false;
        }

        public void SetInputKeyMap(int key, int keyCode)
        {
        }
    }
}