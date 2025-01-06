using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TaoTie
{
    /// <summary>
    /// UI绑定按键
    /// </summary>
    public class PointerInputBind: MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public KeyCode BindingKey;
        public static MultiMapSet<KeyCode, PointerInputBind> Key = new MultiMapSet<KeyCode, PointerInputBind>();
        public static MultiMapSet<KeyCode, PointerInputBind> KeyDown = new MultiMapSet<KeyCode, PointerInputBind>();
        public static MultiMapSet<KeyCode, PointerInputBind> KeyUp = new MultiMapSet<KeyCode, PointerInputBind>();
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