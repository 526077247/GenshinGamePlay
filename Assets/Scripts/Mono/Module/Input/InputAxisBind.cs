using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TaoTie
{
    public enum AxisType
    {
        Vertical,
        Horizontal
    }
    public class InputAxisBind: MonoBehaviour
    {
        public Drag handle;

        private RectTransform background;
        private RectTransform dragTrans;
        public AxisType xBindType = AxisType.Horizontal;
        public AxisType yBindType = AxisType.Vertical;
        
        public static Dictionary<AxisType, float> AxisBind = new Dictionary<AxisType, float>();

        private Vector2 startPos;
        private void OnEnable()
        {
            background = transform as RectTransform;
            dragTrans = handle.transform as RectTransform;
            handle.OnBeginDragHandler.AddListener(OnBeginDragHandler);
            handle.OnDragHandler.AddListener(OnDragHandler);
            handle.OnEndDragHandler.AddListener(OnEndDragHandler);
        }
        
        private void OnDisable()
        {
            handle.OnBeginDragHandler.RemoveListener(OnBeginDragHandler);
            handle.OnDragHandler.RemoveListener(OnDragHandler);
            handle.OnEndDragHandler.RemoveListener(OnEndDragHandler);
        }

        private void OnBeginDragHandler(PointerEventData eventData)
        {
            startPos = eventData.position;
        }
        private void OnEndDragHandler(PointerEventData eventData)
        {
            AxisBind[xBindType] = 0;
            AxisBind[yBindType] = 0;
            dragTrans.anchoredPosition = Vector2.zero;
        }
        private void OnDragHandler(PointerEventData eventData)
        {
            Vector2 touchPosition = eventData.position - startPos;
            // 获取触摸位置相对于摇杆背景的百分比
            touchPosition.x = (touchPosition.x / background.sizeDelta.x)*2;
            touchPosition.y = (touchPosition.y / background.sizeDelta.y)*2;
            touchPosition = (touchPosition.magnitude > 1f) ? touchPosition.normalized : touchPosition;
        
            // 更新摇杆手柄的位置
            dragTrans.anchoredPosition = new Vector2(touchPosition.x * (background.sizeDelta.x / 2), touchPosition.y * (background.sizeDelta.y / 2));
            // 更新输入方向
            AxisBind[xBindType] = touchPosition.x;
            AxisBind[yBindType] = touchPosition.y;
        }

    }
}