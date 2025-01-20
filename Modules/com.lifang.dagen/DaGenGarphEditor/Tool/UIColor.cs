using UnityEditor;
using UnityEngine;

namespace DaGenGraph.Editor
{
    [CreateAssetMenu(fileName = "UIColor", menuName = "DaGenGraph/UIColor", order = 0)]
    public class UIColor : ScriptableObject
    {
        [Tooltip("Node的边缘光颜色")] public Color nodeGlowColor;
        [Tooltip("Node的页眉和页脚背景颜色")] public Color nodeHeaderAndFooterBackgroundColor;
        [Tooltip("Node的主体颜色")] public Color nodeBodyColor;
        [Tooltip("Node的选中框颜色")] public Color nodeOutlineColor;
        [Tooltip("Node的运行状态下选中框的颜色")] public Color nodePlayingOutlineColor;
        [Tooltip("Node的Icon颜色")] public Color nodeHeaderIconColor;
        [Tooltip("Node的分割线颜色")] public Color nodeDividerColor;
        [Tooltip("InputPort的颜色")] public Color portInputColor;
        [Tooltip("OutputPort的颜色")] public Color portOutputColor;
        [Tooltip("InputEdge被选中的颜色")] public Color edgeInputColor;
        [Tooltip("OutputEdge被选中的颜色")] public Color edgeOutputColor;
        [Tooltip("Edge正常的颜色")] public Color edgeNormalColor;
    }

    public class UColor
    {
        private static UIColor s_UIColor;

        private static UIColor uiColor
        {
            get
            {
                if (s_UIColor == null)
                {
                    s_UIColor = AssetDatabase.LoadAssetAtPath<UIColor>("Packages/com.lifang.dagen/Style/UIColor.asset");
                }

                return s_UIColor;
            }
        }

        public static UIColor GetColor()
        {
            return uiColor;
        }
    }
}