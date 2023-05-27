using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DaGenGraph.Editor
{
    public class Styles
    {
        private static readonly string DarkSkinPath = "Packages/com.lifang.dagen/Style/DarkSkin.guiskin";

        private static GUISkin s_darkSkin;

        private static GUISkin DarkSkin =>
            s_darkSkin == null ?
                (s_darkSkin = AssetDatabase.LoadAssetAtPath<GUISkin>(DarkSkinPath))
                : s_darkSkin;


        private static Dictionary<string, GUIStyle> s_darkStyles;

        private static Dictionary<string, GUIStyle> DarkStyles
        {
            get { return s_darkStyles ?? (s_darkStyles = new Dictionary<string, GUIStyle>()); }
        }
        
        public static GUIStyle GetStyle(string styleName)
        {
            if (DarkStyles.ContainsKey(styleName)) return DarkStyles[styleName];
            var newDarkStyle = DarkSkin.GetStyle(styleName);
            if (newDarkStyle != null) DarkStyles.Add(styleName, newDarkStyle);
            return new GUIStyle(newDarkStyle);
        }
    }
}