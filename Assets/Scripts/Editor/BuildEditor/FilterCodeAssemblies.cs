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
            var config = Resources.Load<CDNConfig>("CDNConfig");
            if (config != null) buildHotfixAssembliesAOT = config.BuildHotfixAssembliesAOT;
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
