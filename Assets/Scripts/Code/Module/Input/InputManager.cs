using System;
using System.Collections.Generic;
using Nino.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TaoTie
{
    public class InputManager : IManager, IUpdate
    {
        public const int KeyDown = 1;
        public const int KeyUp = 2;
        public const int Key = 4;
        //这是默认按键配置，实际上会被配置覆盖
        public static readonly KeyCode[] Default = new KeyCode[GameKeyCode.Max]
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
            KeyCode.LeftShift,
        };
        
        public static InputManager Instance { get; private set; }
        public bool IsPause;

        public float MouseScrollWheel { get; private set; }
        public float MouseAxisX{ get; private set; }
        public float MouseAxisY{ get; private set; }

        /// <summary>
        /// 按键绑定
        /// </summary>
        private readonly KeyCode[] keySetMap = new KeyCode[GameKeyCode.Max];

        /// <summary>
        /// 按键状态
        /// </summary>
        private readonly int[] keyStatus = new int[GameKeyCode.Max];
        
        private Vector2 mousePosition;
        private TouchInfo oldTouch1;
        private TouchInfo oldTouch2;

        private readonly List<TouchInfo> touchInfos = new List<TouchInfo>();
        private readonly Dictionary<int, TouchInfo> touchMap = new Dictionary<int, TouchInfo>();
        #region IManager

        public void Init()
        {
            //Input.gyro.enabled = true;
            Instance = this;
            InputKeyBind.Key.Clear();
            InputKeyBind.KeyDown.Clear();
            InputKeyBind.KeyUp.Clear();
            //todo:
            for (int i = 0; i < GameKeyCode.Max; i++)
            {
                keySetMap[i] = Default[i];
            }
        }
        private async ETTask<ConfigInput> GetConfig(string path = "EditConfig/OthersBuildIn/ConfigInput")
        {
            if (Define.ConfigType == 0)
            {
                var jStr = await ResourcesManager.Instance.LoadConfigJsonAsync(path);
                return JsonHelper.FromJson<ConfigInput>(jStr);
            }
            else
            {
                var bytes = await ResourcesManager.Instance.LoadConfigBytesAsync(path);
                return NinoDeserializer.Deserialize<ConfigInput>(bytes);
            }
        }
        public async ETTask LoadAsync()
        {
            var config = await GetConfig("EditConfig/OthersBuildIn/ConfigInput");
            if (config?.Config != null)
            {
                for (int i = 0; i < config.Config.Length; i++)
                {
                    if (PlatformUtil.IsMobile())
                        keySetMap[config.Config[i].GameBehavior] = config.Config[i].Mobile;
                    else
                        keySetMap[config.Config[i].GameBehavior] = config.Config[i].PC;

                }
            }
        }
        public void Destroy()
        {
            Instance = null;
            touchInfos.Clear();
            touchMap.Clear();
            Array.Clear(keySetMap,0,GameKeyCode.Max);
            Array.Clear(keyStatus,0,GameKeyCode.Max);
        }

        #endregion

        public void Update()
        {
            UpdateTouches();
            Array.Clear(keyStatus, 0, GameKeyCode.Max);
            MouseScrollWheel = 0;
            MouseAxisX = 0;
            MouseAxisY = 0;

            if (!IsPause)
            {
                int clickValue = 0;
                if (PlatformUtil.IsMobile())
                {
                    if (Input.touchCount > 0)
                    {
                        mousePosition = Input.GetTouch(0).position;
                    }

                    if (Input.touchCount > 0)
                    {
                        clickValue |= Key;
                    }

                    if (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Began)
                    {
                        clickValue |= KeyDown;
                    }

                    if (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Ended)
                    {
                        clickValue |= KeyUp;
                    }
                }

                for (int i = 0; i < GameKeyCode.Max; ++i)
                {
                    KeyCode key = keySetMap[i];
                    int val = 0;
                    if (key >= 0)
                    {
                        if (PlatformUtil.IsMobile() && key == KeyCode.Mouse0)
                        {
                            val |= clickValue;
                        }
                        else
                        {
                            if (Input.GetKeyDown(key))
                                val |= KeyDown;
                            if (Input.GetKeyUp(key))
                                val |= KeyUp;
                            if (Input.GetKey(key))
                                val |= Key;
                        }

                        var gameKey = i;
                        if (InputKeyBind.KeyDown[gameKey].Count > 0)
                            val |= KeyDown;
                        if (InputKeyBind.KeyUp[gameKey].Count > 0)
                            val |= KeyUp;
                        if (InputKeyBind.Key[gameKey].Count > 0)
                            val |= Key;
                    }

                    if (keyStatus[i] != val)
                    {
                        keyStatus[i] = val;
                        Messager.Instance.Broadcast(0, MessageId.OnKeyInput, i, val);
                    }
                }

                UpdateMouse();
            }
        }

        /// <summary>
        /// 每帧更新触摸信息，维护 touchInfos 和 touchMap
        /// </summary>
        private void UpdateTouches()
        {
            // 清空当前帧的触摸列表（准备重新填充）
            touchInfos.Clear();

            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch touch = Input.GetTouch(i);
                int fingerId = touch.fingerId;

                // 尝试从字典获取已有的 TouchInfo
                if (!touchMap.TryGetValue(fingerId, out TouchInfo info))
                {
                    // 新触摸：从对象池获取
                    info = ObjectPool.Instance.Fetch<TouchInfo>();
                    info.IsStartOverUI = IsPointerOverUI(touch.position);
                    info.IsScroll = PlatformUtil.IsSimulator(); // 模拟器初始假设为滚轮
                    touchMap.Add(fingerId, info);
                }

                // 更新 TouchInfo 的当前触摸数据（阶段、位置、增量等）
                info.Phase = touch.phase;
                info.Position = touch.position;
                info.DeltaPosition = touch.deltaPosition;

                // 更新 IsScroll 标志（模拟器下特殊处理）
                if (PlatformUtil.IsSimulator() && touch.phase == TouchPhase.Moved)
                {
                    // 水平方向有移动或竖直移动量很小 -> 不是滚轮
                    if (Mathf.Abs(touch.deltaPosition.x) > 0.01f || Mathf.Abs(touch.deltaPosition.y) < 5f)
                    {
                        info.IsScroll = false;
                    }
                }

                // 将当前触摸加入列表（供外部使用）
                touchInfos.Add(info);

                // 若触摸结束，从字典移除并回收对象
                if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                {
                    touchMap.Remove(fingerId);
                    info.Dispose(); // 归还对象池
                }
            }
        }
        
        /// <summary>
        /// 获取和UI无关的触碰
        /// </summary>
        /// <returns></returns>
        private ListComponent<TouchInfo> GetNoneUITouch()
        {
            ListComponent<TouchInfo> res = ListComponent<TouchInfo>.Create();
            for (int i = 0; i < touchInfos.Count; i++)
            {
                if (!touchInfos[i].IsStartOverUI)
                {
                    res.Add(touchInfos[i]);
                }
            }

            return res;
        }

        private void UpdateMouse()
        {
            if (PlatformUtil.IsMobile())
            {
                using var touchInfos = GetNoneUITouch();
                if (touchInfos.Count == 1)
                {
                    var touch = touchInfos[0];
                    if(touch == null) return;
                    if (touch.Phase == TouchPhase.Moved)
                    {
                        if (touchInfos[0].IsScroll)
                        {
                            MouseScrollWheel = touch.DeltaPosition.y / 100;
                        }
                        else
                        {
                            //webgl移动端是反的
#if UNITY_WEBGL
                            MouseAxisX = -touch.DeltaPosition.x / 50;
                            MouseAxisY = -touch.DeltaPosition.y / 50;
#else
                            MouseAxisX = touch.DeltaPosition.x / 50;
                            MouseAxisY = touch.DeltaPosition.y / 50;    
#endif
                        }
                    }

                }
                else if (touchInfos.Count == 2)
                {
                    var newTouch1 = touchInfos[0];
                    var newTouch2 = touchInfos[1];
                    if (newTouch2.Phase == TouchPhase.Began)
                    {
                        oldTouch2 = newTouch2;
                        oldTouch1 = newTouch1;
                        return;
                    }

                    float oldDistance = Vector2.Distance(oldTouch1.Position, oldTouch2.Position);
                    float newDistance = Vector2.Distance(newTouch1.Position, newTouch2.Position);
                    float offset = newDistance - oldDistance;

                    if (Mathf.Abs(offset) >= 3)
                    {
                        MouseScrollWheel = offset / 100;
                        oldTouch1 = newTouch1;
                        oldTouch2 = newTouch2;
                    }
                }
            }
            else
            {
                MouseScrollWheel = Input.GetAxis("Mouse ScrollWheel");
                MouseAxisX = Input.GetAxis("Mouse X");
                MouseAxisY = Input.GetAxis("Mouse Y");
            }
        }
        /// <summary>
        /// 获取按键
        /// </summary>
        /// <param name="keyCode"></param>
        /// <param name="ignoreUI"></param>
        /// <returns></returns>
        public bool GetKey(int keyCode, bool ignoreUI = false)
        {
            if ((keyStatus[keyCode] & Key) != 0)
            {
                if (ignoreUI && InputKeyBind.Key[keyCode].Count == 0)
                {
                    if (PlatformUtil.IsMobile() && Input.touchCount == 0)
                    {
                        return true;
                    }
                    var pos = GetLastTouchPos();
                    return !IsPointerOverUI(pos);
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
        public bool GetKeyDown(int keyCode, bool ignoreUI = false)
        {
            if ((keyStatus[keyCode] & KeyDown) != 0)
            {
                if (ignoreUI && InputKeyBind.KeyDown[keyCode].Count == 0)
                {
                    if (PlatformUtil.IsMobile() && Input.touchCount == 0)
                    {
                        return true;
                    }
                    var pos = GetLastTouchPos();
                    return !IsPointerOverUI(pos);
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
        public bool GetKeyUp(int keyCode, bool ignoreUI = false)
        {
            if ((keyStatus[keyCode] & KeyUp) != 0)
            {
                if (ignoreUI && InputKeyBind.KeyUp[keyCode].Count == 0)
                {
                    if (PlatformUtil.IsMobile() && Input.touchCount == 0)
                    {
                        return true;
                    }
                    var pos = GetLastTouchPos();
                    return !IsPointerOverUI(pos);
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
        public bool GetKey(params int[] keyCodes)
        {
            for (int i = 0; i < keyCodes.Length; i++)
            {
                if ((keyStatus[keyCodes[i]] & Key) != 0)
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
        public bool GetKeyDown(params int[] keyCodes)
        {
            for (int i = 0; i < keyCodes.Length; i++)
            {
                if ((keyStatus[keyCodes[i]] & KeyDown) != 0)
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
        public bool GetKeyUp(params int[] keyCodes)
        {
            for (int i = 0; i < keyCodes.Length; i++)
            {
                if ((keyStatus[keyCodes[i]] & KeyUp) != 0)
                {
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        ///  获取其他按键
        /// </summary>
        /// <param name="exceptKeyCode"></param>
        /// <returns></returns>
        public bool GetAnyKeyExcept(int exceptKeyCode)
        {
            for (int i = 0; i < keyStatus.Length; i++)
            {
                if ((keyStatus[i] & Key) != 0)
                {
                    if (exceptKeyCode == i)
                    {
                        continue;
                    }
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        ///  获取其他按键是否按下
        /// </summary>
        /// <param name="exceptKeyCode"></param>
        /// <returns></returns>
        public bool GetAnyKeyDownExcept(int exceptKeyCode)
        {
            for (int i = 0; i < keyStatus.Length; i++)
            {
                if ((keyStatus[i] & KeyDown) != 0)
                {
                    if (exceptKeyCode == i)
                    {
                        continue;
                    }
                    return true;
                }
            }
            return false;
        }
        
        /// <summary>
        ///  获取其他按键是否抬起
        /// </summary>
        /// <param name="exceptKeyCode"></param>
        /// <returns></returns>
        public bool GetAnyKeyUpExcept(int exceptKeyCode)
        {
            for (int i = 0; i < keyStatus.Length; i++)
            {
                if ((keyStatus[i] & KeyUp) != 0)
                {
                    if (exceptKeyCode == i)
                    {
                        continue;
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
        public bool GetAnyKeyExcept(params int[] exceptKeyCodes)
        {
            for (int i = 0; i < keyStatus.Length; i++)
            {
                if ((keyStatus[i] & Key) != 0)
                {
                    if (exceptKeyCodes == null || exceptKeyCodes.Length == 0) return true;
                    bool has = false;
                    for (int j = 0; j < exceptKeyCodes.Length; j++)
                    {
                        if (exceptKeyCodes[j] == i)
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
        public bool GetAnyKeyDownExcept(params int[] exceptKeyCodes)
        {
            for (int i = 0; i < keyStatus.Length; i++)
            {
                if ((keyStatus[i] & KeyDown) != 0)
                {
                    if (exceptKeyCodes == null || exceptKeyCodes.Length == 0) return true;
                    bool has = false;
                    for (int j = 0; j < exceptKeyCodes.Length; j++)
                    {
                        if (exceptKeyCodes[j] == i)
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
        public bool GetAnyKeyUpExcept(params int[] exceptKeyCodes)
        {
            for (int i = 0; i < keyStatus.Length; i++)
            {
                if ((keyStatus[i] & KeyUp) != 0)
                {
                    if (exceptKeyCodes == null || exceptKeyCodes.Length == 0) return true;
                    bool has = false;
                    for (int j = 0; j < exceptKeyCodes.Length; j++)
                    {
                        if (exceptKeyCodes[j] == i)
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
        
        public Vector2 GetLastTouchPos()
        {
            if (PlatformUtil.IsMobile())
            {
                return mousePosition;
            }
            var data = Input.mousePosition;
            return new Vector2(data.x, data.y);

        }

        /// <summary>
        /// 按键映射
        /// </summary>
        /// <param name="key"></param>
        /// <param name="keyCode"></param>
        public void SetInputKeyMap(int key, KeyCode keyCode)
        {
            keySetMap[key] = keyCode;
        }
        
        public bool IsPointerOverUI(Vector2 mousePosition, params RectTransform[] ignore)
        {       
            //创建一个点击事件
            PointerEventData eventData = new PointerEventData(EventSystem.current);
            eventData.position = mousePosition;
            using ListComponent<RaycastResult> raycastResults = ListComponent<RaycastResult>.Create();
            //向点击位置发射一条射线，检测是否点击UI
            EventSystem.current.RaycastAll(eventData, raycastResults);
            if (raycastResults.Count > 0)
            {
                if (ignore != null && ignore.Length > 0)
                {
                    for (int i = 0; i < raycastResults.Count; i++)
                    {
                        for (int j = 0; j < ignore.Length; j++)
                        {
                            if (raycastResults[i].gameObject != ignore[j].gameObject)
                            {
                                return true;
                            }
                        }
                    }
                    return false;
                }
                return true;
            }
            return false;
        }
        
        public bool IsPointerOverButton(Vector2 mousePosition)
        {       
            //创建一个点击事件
            PointerEventData eventData = new PointerEventData(EventSystem.current);
            eventData.position = mousePosition;
            using ListComponent<RaycastResult> raycastResults = ListComponent<RaycastResult>.Create();
            //向点击位置发射一条射线，检测是否点击UI
            EventSystem.current.RaycastAll(eventData, raycastResults);
            if (raycastResults.Count > 0)
            {
                return raycastResults[0].gameObject.GetComponent<Button>() != null
                       || raycastResults[0].gameObject.GetComponent<PointerClick>() != null;
            }
            return false;
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