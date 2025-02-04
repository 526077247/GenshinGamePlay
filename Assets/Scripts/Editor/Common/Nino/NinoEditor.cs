using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace TaoTie
{
    public static class NinoEditor
    {
        private static string roslynAnalyzer = "RoslynAnalyzer";
        [MenuItem("Tools/Build/关闭Nino二进制输出")]
        static void CloseExportBytes()
        {
            var buildTarget = BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget);
            var symbolStr = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTarget);
            symbolStr = symbolStr.Replace(roslynAnalyzer, "");
            symbolStr = symbolStr.Replace(";;", "");
            PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTarget,symbolStr);
            var dll = AssetDatabase.LoadAssetAtPath<Object>("Assets/Scripts/ThirdParty/Nino/Nino.Generator.dll");
            AssetDatabase.ClearLabels(dll);
        }
        [MenuItem("Tools/Build/开启Nino二进制输出")]
        static void OpenExportBytes()
        {
            var buildTarget = BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget);
            var symbolStr = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTarget);
            if (string.IsNullOrEmpty(symbolStr))
            {
                symbolStr = roslynAnalyzer;
            }
            else
            {
                symbolStr += ";" + roslynAnalyzer;
            }
            PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTarget,symbolStr);
            var dll = AssetDatabase.LoadAssetAtPath<Object>("Assets/Scripts/ThirdParty/Nino/Nino.Generator.dll");
            AssetDatabase.SetLabels(dll,new []{roslynAnalyzer});
        }
    }
}