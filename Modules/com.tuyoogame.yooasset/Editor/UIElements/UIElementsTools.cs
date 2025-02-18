#if UNITY_2019_4_OR_NEWER
using UnityEditor;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace YooAsset.Editor
{
    public static class UIElementsTools
    {
        /// <summary>
        /// 设置元素显隐
        /// </summary>
        public static void SetElementVisible(VisualElement element, bool visible)
        {
            if (element == null)
                return;

            element.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
            element.style.visibility = visible ? Visibility.Visible : Visibility.Hidden;
        }

        /// <summary>
        /// 设置元素的文本最小宽度
        /// </summary>
        public static void SetElementLabelMinWidth(VisualElement element, int minWidth)
        {
            var label = element.Q<Label>();
            if (label != null)
            {
                // 设置最小宽度
                label.style.minWidth = minWidth;
            }
        }
    }
}
#endif