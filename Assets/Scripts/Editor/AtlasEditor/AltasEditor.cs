using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;

namespace TaoTie
{
    public class AltasEditor
    {
        [MenuItem("Tools/UI/生成图集", false, 30)]
        public static void GeneratingAtlas()
        {
            AltasHelper.GeneratingAtlas();
        }

        [MenuItem("Tools/UI/清理图集", false, 30)]
        public static void ClearAllAtlas()
        {
            AltasHelper.ClearAllAtlas();
        }

        [MenuItem("Tools/UI/设置图片", false, 31)]
        public static void SettingPNG()
        {
            AltasHelper.SettingPNG();
        }

        [MenuItem("Tools/UI/清理和生成图集", false, 32)]
        public static void ClearAllAtlasAndGenerate()
        {
            AltasHelper.ClearAllAtlas();
            AltasHelper.GeneratingAtlas();
        }
    }
}
