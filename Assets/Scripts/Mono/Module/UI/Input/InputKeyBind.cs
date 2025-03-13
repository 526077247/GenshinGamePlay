using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TaoTie
{
    /// <summary>
    /// UI绑定按键
    /// </summary>
    public class InputKeyBind: MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public KeyCode BindingKey;
        public static MultiMapSet<KeyCode, InputKeyBind> Key = new MultiMapSet<KeyCode, InputKeyBind>();
        public static MultiMapSet<KeyCode, InputKeyBind> KeyDown = new MultiMapSet<KeyCode, InputKeyBind>();
        public static MultiMapSet<KeyCode, InputKeyBind> KeyUp = new MultiMapSet<KeyCode, InputKeyBind>();
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