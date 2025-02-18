#if UNITY_2019_4_OR_NEWER
using System;
using UnityEditor;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace YooAsset.Editor
{
    /// <summary>
    /// 显示开关（眼睛图标）
    /// </summary>
    public class DisplayToggle : Toggle
    {
        private readonly VisualElement _checkbox;

        public DisplayToggle()
        {
            _checkbox = this.Q<VisualElement>("unity-checkmark");
            RefreshIcon();
        }

        /// <summary>
        /// 刷新图标
        /// </summary>
        public void RefreshIcon()
        {
            if (this.value)
            {
                var icon = EditorGUIUtility.IconContent("animationvisibilitytoggleoff@2x").image as Texture2D;
                _checkbox.style.backgroundImage = icon;
            }
            else
            {
                var icon = EditorGUIUtility.IconContent("animationvisibilitytoggleon@2x").image as Texture2D;
                _checkbox.style.backgroundImage = icon;
            }
        }
    }
}
#endif