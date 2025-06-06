#if ODIN_INSPECTOR
using System.IO;
using UnityEditor;

namespace TaoTie
{
    public static class OdinGenerate
    {
        [InitializeOnLoadMethod]
        private static void InitializeOnLoadMethod()
        {
            //Odin放Packages里一直在生成
            if (Directory.Exists("Assets/Packages"))
            {
                Directory.Delete("Assets/Packages",true);
                File.Delete("Assets/Packages.meta");
                AssetDatabase.Refresh();
            }
        }

    }
}
#endif