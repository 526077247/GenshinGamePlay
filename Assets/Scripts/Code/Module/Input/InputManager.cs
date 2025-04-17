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
            KeyCode.LeftAlt,
            KeyCode.Q,
            KeyCode.E,
            KeyCode.Escape,
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

        private int touchCount = 0;
#if !UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS)
        private Vector2 mousePosition;
#endif
        #region IManager

        public void Init()
        {
            //Input.gyro.enabled = true;
            Instance = this;
            InputKeyBind.Key.Clear();
            InputKeyBind.KeyDown.Clear();
            InputKeyBind.KeyUp.Clear();
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
#if !UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS)
            if (Input.touchCount > 0)
            {
                mousePosition = Input.GetTouch(0).position;
            }
            int clickValue = 0;
            if (Input.touchCount > 0|| InputKeyBind.Key[KeyCode.Mouse0].Count > 0)
            {
                clickValue |= Key;
            }
            if ((Input.touchCount > 0 && touchCount == 0)|| InputKeyBind.KeyDown[KeyCode.Mouse0].Count > 0)
            {
                clickValue |= KeyDown;
            }
            if ((Input.touchCount == 0 && touchCount > 0)|| InputKeyBind.KeyUp[KeyCode.Mouse0].Count > 0)
            {
                clickValue |= KeyUp;
            }
            touchCount = Input.touchCount;
#endif
            
            for (int i= 0; i< (int)GameKeyCode.Max; ++i)
            {
                KeyCode key = keySetMap[i];
                int val = 0;
                if (key >= 0)
                {
#if !UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS)
                    if (key == KeyCode.Mouse0)
                    {
                        val |= clickValue;
                    } else {
#endif
                    if (Input.GetKeyDown(key) || InputKeyBind.KeyDown[key].Count > 0)
                        val |= KeyDown;
                    if (Input.GetKeyUp(key) || InputKeyBind.KeyUp[key].Count > 0)
                        val |= KeyUp;
                    if (Input.GetKey(key) || InputKeyBind.Key[key].Count > 0)
                        val |= Key;
#if !UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS)
                    }
#endif
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
        /// 获取按键
        /// </summary>
        /// <param name="keyCode"></param>
        /// <param name="ignoreUI"></param>
        /// <returns></returns>
        public bool GetKey(GameKeyCode keyCode, bool ignoreUI = false)
        {
            if ((keyStatus[(int) keyCode] & Key) != 0)
            {
                if (ignoreUI)
                {
                    var pos = GetTouchPos();
                    return !IsPointerOverGameObject(pos);
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// 获取按键是否按下
        /// </summary>
        /// <param name="keyCode"></param>
        /// <param name="ignoreUI"></param>
        /// <returns></returns>
        public bool GetKeyDown(GameKeyCode keyCode, bool ignoreUI = false)
        {
            if ((keyStatus[(int) keyCode] & KeyDown) != 0)
            {
                if (ignoreUI)
                {
                    var pos = GetTouchPos();
                    return !IsPointerOverGameObject(pos);
                }
                return true;
            }
            return false;
        }
        
        /// <summary>
        /// 获取按键是否抬起
        /// </summary>
        /// <param name="keyCode"></param>
        /// <param name="ignoreUI"></param>
        /// <returns></returns>
        public bool GetKeyUp(GameKeyCode keyCode, bool ignoreUI = false)
        {
            if ((keyStatus[(int) keyCode] & KeyUp) != 0)
            {
                if (ignoreUI)
                {
                    var pos = GetTouchPos();
                    return !IsPointerOverGameObject(pos);
                }
                return true;
            }
            return false;
        }
        /// <summary>
        ///  获取按键
        /// </summary>
        /// <param name="keyCodes"></param>
        /// <returns></returns>
        public bool GetKey(params GameKeyCode[] keyCodes)
        {
            for (int i = 0; i < keyCodes.Length; i++)
            {
                if ((keyStatus[(int) keyCodes[i]] & Key) != 0)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        ///  获取按键是否按下
        /// </summary>
        /// <param name="keyCodes"></param>
        /// <returns></returns>
        public bool GetKeyDown(params GameKeyCode[] keyCodes)
        {
            for (int i = 0; i < keyCodes.Length; i++)
            {
                if ((keyStatus[(int) keyCodes[i]] & KeyDown) != 0)
                {
                    return true;
                }
            }
            return false;
        }
        
        /// <summary>
        ///  获取按键是否抬起
        /// </summary>
        /// <param name="keyCodes"></param>
        /// <returns></returns>
        public bool GetKeyUp(params GameKeyCode[] keyCodes)
        {
            for (int i = 0; i < keyCodes.Length; i++)
            {
                if ((keyStatus[(int) keyCodes[i]] & KeyUp) != 0)
                {
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        ///  获取其他按键
        /// </summary>
        /// <param name="exceptKeyCode">不传为任意</param>
        /// <returns></returns>
        public bool GetAnyKeyExcept(GameKeyCode exceptKeyCode)
        {
            for (int i = 0; i < keyStatus.Length; i++)
            {
                if ((keyStatus[i] & Key) != 0)
                {
                    if ((int) exceptKeyCode == i)
                    {
                        return false;
                    }
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        ///  获取其他按键是否按下
        /// </summary>
        /// <param name="exceptKeyCode">不传为任意</param>
        /// <returns></returns>
        public bool GetAnyKeyDownExcept(GameKeyCode exceptKeyCode)
        {
            for (int i = 0; i < keyStatus.Length; i++)
            {
                if ((keyStatus[i] & KeyDown) != 0)
                {
                    if ((int) exceptKeyCode == i)
                    {
                        return false;
                    }
                    return true;
                }
            }
            return false;
        }
        
        /// <summary>
        ///  获取其他按键是否抬起
        /// </summary>
        /// <param name="exceptKeyCode">不传为任意</param>
        /// <returns></returns>
        public bool GetAnyKeyUpExcept(GameKeyCode exceptKeyCode)
        {
            for (int i = 0; i < keyStatus.Length; i++)
            {
                if ((keyStatus[i] & KeyUp) != 0)
                {
                    if ((int) exceptKeyCode == i)
                    {
                        return false;
                    }
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        ///  获取其他按键
        /// </summary>
        /// <param name="exceptKeyCodes">不传为任意</param>
        /// <returns></returns>
        public bool GetAnyKeyExcept(params GameKeyCode[] exceptKeyCodes)
        {
            for (int i = 0; i < keyStatus.Length; i++)
            {
                if ((keyStatus[i] & Key) != 0)
                {
                    if (exceptKeyCodes == null || exceptKeyCodes.Length == 0) return true;
                    bool has = false;
                    for (int j = 0; j < exceptKeyCodes.Length; j++)
                    {
                        if ((int) exceptKeyCodes[j] == i)
                        {
                            has = true;
                            break;
                        }
                    }

                    if (!has) return true;
                }
            }
            return false;
        }

        /// <summary>
        ///  获取其他按键是否按下
        /// </summary>
        /// <param name="exceptKeyCodes">不传为任意</param>
        /// <returns></returns>
        public bool GetAnyKeyDownExcept(params GameKeyCode[] exceptKeyCodes)
        {
            for (int i = 0; i < keyStatus.Length; i++)
            {
                if ((keyStatus[i] & KeyDown) != 0)
                {
                    if (exceptKeyCodes == null || exceptKeyCodes.Length == 0) return true;
                    bool has = false;
                    for (int j = 0; j < exceptKeyCodes.Length; j++)
                    {
                        if ((int) exceptKeyCodes[j] == i)
                        {
                            has = true;
                            break;
                        }
                    }

                    if (!has) return true;
                }
            }
            return false;
        }
        
        /// <summary>
        ///  获取其他按键是否抬起
        /// </summary>
        /// <param name="exceptKeyCodes">不传为任意</param>
        /// <returns></returns>
        public bool GetAnyKeyUpExcept(params GameKeyCode[] exceptKeyCodes)
        {
            for (int i = 0; i < keyStatus.Length; i++)
            {
                if ((keyStatus[i] & KeyUp) != 0)
                {
                    if (exceptKeyCodes == null || exceptKeyCodes.Length == 0) return true;
                    bool has = false;
                    for (int j = 0; j < exceptKeyCodes.Length; j++)
                    {
                        if ((int) exceptKeyCodes[j] == i)
                        {
                            has = true;
                            break;
                        }
                    }

                    if (!has) return true;
                }
            }
            return false;
        }
        
        public Vector2 GetTouchPos()
        {
#if !UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS)
            return mousePosition;
#else
            var data = Input.mousePosition;
            return new Vector2(data.x, data.y);
#endif
        }

        /// <summary>
        /// 按键映射
        /// </summary>
        /// <param name="key"></param>
        /// <param name="keyCode"></param>
        public void SetInputKeyMap(GameKeyCode key, KeyCode keyCode)
        {
            keySetMap[(int) key] = keyCode;
        }
        

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

        public bool IsPointerOverUI(Vector2 mousePosition)
        {       
            //创建一个点击事件
            PointerEventData eventData = new PointerEventData(EventSystem.current);
            eventData.position = mousePosition;
            List<RaycastResult> raycastResults = new List<RaycastResult>();
            //向点击位置发射一条射线，检测是否点击UI
            EventSystem.current.RaycastAll(eventData, raycastResults);
            return raycastResults.Count > 0;
        }
        
        #region Gyroscope

        /// <summary>
        /// 获取陀螺仪参数
        /// </summary>
        /// <returns></returns>
        public Quaternion GetGyroAttitude()
        {
            return Input.gyro.attitude;
        }
        
        #endregion
    }
}