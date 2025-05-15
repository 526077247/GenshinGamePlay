using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TaoTie
{
    /// <summary>
    /// UI绑定按键
    /// </summary>
    public class InputKeyBind: MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
#if UNITY_EDITOR
        [ValueDropdown("@"+nameof(GameKeyCode)+"."+nameof(GameKeyCode.GetGameKeyCodeList)+"()")]
#endif
        public int BindingKey;
        public static MultiMapSet<int, InputKeyBind> Key = new MultiMapSet<int, InputKeyBind>();
        public static MultiMapSet<int, InputKeyBind> KeyDown = new MultiMapSet<int, InputKeyBind>();
        public static MultiMapSet<int, InputKeyBind> KeyUp = new MultiMapSet<int, InputKeyBind>();
        public void LateUpdate()
        {
            KeyDown.Remove(BindingKey,this);
            KeyUp.Remove(BindingKey,this);
        }

        public void OnDisable()
        {
            Key.Remove(BindingKey,this);
            KeyDown.Remove(BindingKey,this);
            KeyUp.Remove(BindingKey,this);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            Key.Add(BindingKey,this);
            KeyDown.Add(BindingKey,this);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            Key.Remove(BindingKey,this);
            KeyUp.Add(BindingKey,this);
        }
    }
}