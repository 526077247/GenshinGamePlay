using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace TaoTie
{
    public class Drag: MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
    {
        public UnityEvent<PointerEventData> OnBeginDragHandler = new UnityEvent<PointerEventData>();
        public UnityEvent<PointerEventData> OnDragHandler = new UnityEvent<PointerEventData>();
        public UnityEvent<PointerEventData> OnEndDragHandler = new UnityEvent<PointerEventData>();
        public void OnBeginDrag(PointerEventData eventData)
        {
            OnBeginDragHandler?.Invoke(eventData);
        }
        public void OnDrag(PointerEventData eventData)
        {
            OnDragHandler?.Invoke(eventData);
        }
        public void OnEndDrag(PointerEventData eventData)
        {
            OnEndDragHandler?.Invoke(eventData);
        }
    }
}