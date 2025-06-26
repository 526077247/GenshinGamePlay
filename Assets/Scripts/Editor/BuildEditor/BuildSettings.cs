using UnityEditor;
using UnityEngine;

namespace TaoTie
{
    public class BuildSettings : ScriptableObject
    {
        public string channel = "googleplay";
        public Mode buildMode = Mode.内网测试;
        public bool clearBuildCache = false;
        public bool clearReleaseFolder = false;
        public bool clearABFolder = false;
        public bool isBuildExe = false;
        public bool buildHotfixAssembliesAOT = true;
        public bool isContainsAb = false;
        public bool isBuildAll = false;
        public bool isPackAtlas = false;
        public string cdn = "";
        public BuildType buildType = BuildType.Release;
        public BuildAssetBundleOptions buildAssetBundleOptions = BuildAssetBundleOptions.None;
    }
}