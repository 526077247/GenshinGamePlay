using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace TaoTie
{
    internal class FilterCodeAssemblies : IFilterBuildAssemblies
    {
        public int callbackOrder => 1;
        
        public string[] OnFilterAssemblies(BuildOptions buildOptions, string[] assemblies)
        {
            bool buildHotfixAssembliesAOT = false;
            var buildSettings = AssetDatabase.LoadAssetAtPath<BuildSettings>(BuildEditor.settingAsset);
            if (buildSettings != null) buildHotfixAssembliesAOT = buildSettings.buildHotfixAssembliesAOT;
            if(buildHotfixAssembliesAOT) return assemblies;
            // 将热更dll从打包列表中移除
            return assemblies.Where(ass =>
            {
                string assName = Path.GetFileNameWithoutExtension(ass);
                bool reserved = !ass.Contains("Unity.Code");
                if (!reserved)
                {
                    Debug.Log($"[FilterHotFixAssemblies] filter assembly:{assName}");
                }
                return reserved;
            }).ToArray();
        }
    }
}
