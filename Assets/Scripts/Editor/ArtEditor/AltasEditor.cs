using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace TaoTie
{
    public class AltasEditor
    {
        [MenuItem("Tools/UI/生成图集", false, 30)]
        public static void GeneratingAtlas()
        {
            AtlasHelper.GeneratingAtlas();
        }

        [MenuItem("Tools/UI/清理图集", false, 30)]
        public static void ClearAllAtlas()
        {
            AtlasHelper.ClearAllAtlas();
        }

        [MenuItem("Tools/UI/设置图片", false, 31)]
        public static void SettingPNG()
        {
            AtlasHelper.SettingPNG();
        }

        [MenuItem("Tools/UI/清理和生成图集", false, 32)]
        public static void ClearAllAtlasAndGenerate()
        {
            AtlasHelper.ClearAllAtlas();
            AtlasHelper.GeneratingAtlas();
        }
        
        
    }
}