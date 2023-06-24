using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TaoTie
{
    public class InputManager : IManager, IUpdate
    {
        public const int KeyDown = 1;
        public const int KeyUp = 2;
        public const int Key = 4;
        public readonly KeyCode[] Default = new KeyCode[(int) GameKeyCode.Max]
        {
            KeyCode.W,
            KeyCode.S,
            KeyCode.A,
            KeyCode.D,
            KeyCode.Space,
            KeyCode.Mouse0,
            KeyCode.F,
        };
        
        public static InputManager Instance { get; private set; }
        public bool IsPause;

        public float MouseScrollWheel { get; private set; }
        public float MouseAxisX{ get; private set; }
        public float MouseAxisY{ get; private set; }

        /// <summary>
        /// 按键绑定
        /// </summary>
        private readonly KeyCode[] keySetMap = new KeyCode[(int)GameKeyCode.Max];

        /// <summary>
        /// 按键状态
        /// </summary>
        private readonly int[] keyStatus = new int[(int)GameKeyCode.Max];

        #region IManager

        public void Init()
        {
            Instance = this;
            //todo:
            for (int i = 0; i < (int)GameKeyCode.Max; i++)
            {
                keySetMap[i] = Default[i];
            }
        }

        public void Destroy()
        {
            Instance = null;
            Array.Clear(keySetMap,0,(int)GameKeyCode.Max);
            Array.Clear(keyStatus,0,(int)GameKeyCode.Max);
        }

        #endregion

        public void Update()
        {
            Array.Clear(keyStatus,0,(int)GameKeyCode.Max);
            MouseScrollWheel = 0;
            MouseAxisX = 0;
            MouseAxisY = 0;
            if (IsPause) return;
            for (int i= 0; i< (int)GameKeyCode.Max; ++i)
            {
                KeyCode key = keySetMap[i];
                int val = 0;
                if (key >= 0)
                {
                    if (Input.GetKeyDown(key))
                        val += KeyDown;
                    if (Input.GetKeyUp(key))
                        val += KeyUp;
                    if (Input.GetKey(key))
                        val += Key;
                }

                if (keyStatus[i] != val)
                {
                    keyStatus[i] = val;
                    Messager.Instance.Broadcast(0, MessageId.OnKeyInput, i, val);
                }
            }

            MouseScrollWheel = Input.GetAxis("Mouse ScrollWheel");
            MouseAxisX = Input.GetAxis("Mouse X");
            MouseAxisY = Input.GetAxis("Mouse Y");
        }

        /// <summary>
        ///  获取按键
        /// </summary>
        /// <param name="keyCode"></param>
        /// <returns></returns>
        public bool GetKey(GameKeyCode keyCode)
        {
            return (keyStatus[(int)keyCode]& Key) != 0;
        }

        /// <summary>
        ///  获取按键是否按下
        /// </summary>
        /// <param name="keyCode"></param>
        /// <returns></returns>
        public bool GetKeyDown(GameKeyCode keyCode)
        {
            return (keyStatus[(int)keyCode]& KeyDown) != 0;
        }
        
        /// <summary>
        ///  获取按键是否抬起
        /// </summary>
        /// <param name="keyCode"></param>
        /// <returns></returns>
        public bool GetKeyUp(GameKeyCode keyCode)
        {
            return (keyStatus[(int)keyCode]& KeyUp) != 0;
        }
        
        public void SetInputKeyMap(GameKeyCode key, KeyCode keyCode)
        {
            keySetMap[(int) key] = keyCode;
        }
        
#if !UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS)
        public bool IsPointerOverGameObject(Vector2 mousePosition)
        {       
            //创建一个点击事件
            PointerEventData eventData = new PointerEventData(EventSystem.current);
            eventData.position = mousePosition;
            List<RaycastResult> raycastResults = new List<RaycastResult>();
            //向点击位置发射一条射线，检测是否点击UI
            EventSystem.current.RaycastAll(eventData, raycastResults);
            if (raycastResults.Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
#endif
    }
}