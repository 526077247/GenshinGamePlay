#if UNITY_2020_3_OR_NEWER
using System;
using UnityEditor;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace YooAsset.Editor
{
    /// <summary>
    /// 分屏控件
    /// </summary>
    public class PanelSplitView : TwoPaneSplitView
    {
        public new class UxmlFactory : UxmlFactory<PanelSplitView, UxmlTraits>
        {
        }

        /// <summary>
        /// 竖版分屏
        /// </summary>
        public static void SplitVerticalPanel(VisualElement root, VisualElement panelA, VisualElement panelB)
        {
            root.Remove(panelA);
            root.Remove(panelB);

            var spliteView = new PanelSplitView();
            spliteView.fixedPaneInitialDimension = 300;
            spliteView.orientation = TwoPaneSplitViewOrientation.Vertical;
            spliteView.contentContainer.Add(panelA);
            spliteView.contentContainer.Add(panelB);
            root.Add(spliteView);
        }
    }
}
#endif