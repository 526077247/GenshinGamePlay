using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace TaoTie
{
    
    /// <summary>
    /// 点击模块
    /// </summary>
    public class PointerClick : MonoBehaviour, IPointerClickHandler
    {
        public UnityEvent onClick = new UnityEvent();

        public void OnPointerClick(PointerEventData eventData)
        {
            onClick.Invoke();
        }
    }
}