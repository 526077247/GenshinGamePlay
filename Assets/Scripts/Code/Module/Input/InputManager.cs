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
            foreach (var keyValue in keySetMap)
            {
                keyStatus.Add(keyValue.Value, false);
            }
        }

        public void Destroy()
        {
            Instance = null;
            keyStatus.Clear();
        }

        #endregion

        public void Update()
        {
            if (IsPause) return;
            foreach (var keyValue in keySetMap)
            {
                var keyCode = (KeyCode)Enum.ToObject(typeof(KeyCode), keyValue.Value);
                keyStatus[keyValue.Value] = Input.GetKey(keyCode);
            }
        }

        public bool InputGetKey(int keyCode)
        {
            return keyStatus[keyCode];
        }
    }
}