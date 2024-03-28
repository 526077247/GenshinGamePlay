using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using YooAsset.Editor;
using YooAsset;
namespace TaoTie
{
    public static class BuildHelper
    {
        const string relativeDirPrefix = "Release";

        public static readonly Dictionary<PlatformType, BuildTarget> buildmap = new Dictionary<PlatformType, BuildTarget>(PlatformTypeComparer.Instance)
        {
            { PlatformType.Android , BuildTarget.Android },
            { PlatformType.Windows , BuildTarget.StandaloneWindows64 },
            { PlatformType.IOS , BuildTarget.iOS },
            { PlatformType.MacOS , BuildTarget.StandaloneOSX },
            { PlatformType.Linux , BuildTarget.StandaloneLinux64 },
        };

        public static readonly Dictionary<PlatformType, BuildTargetGroup> buildGroupmap = new Dictionary<PlatformType, BuildTargetGroup>(PlatformTypeComparer.Instance)
        {
            { PlatformType.Android , BuildTargetGroup.Android },
            { PlatformType.Windows , BuildTargetGroup.Standalone },
            { PlatformType.IOS , BuildTargetGroup.iOS },
            { PlatformType.MacOS , BuildTargetGroup.Standalone },
            { PlatformType.Linux , BuildTargetGroup.Standalone },
        };
        public static void KeystoreSetting()
        {
            PlayerSettings.Android.keystoreName = "TaoTie.keystore";
            PlayerSettings.Android.keyaliasName = "taitie";
            PlayerSettings.keyaliasPass = "123456";
            PlayerSettings.keystorePass = "123456";
        }

        public static void Build(PlatformType type, BuildOptions buildOptions, bool isBuildExe,bool clearFolder,
            bool isBuildAll,bool packAtlas)
        {
            if (buildmap[type] == EditorUserBuildSettings.activeBuildTarget)
            {
                //pack
                BuildHandle(type, buildOptions, isBuildExe,clearFolder,isBuildAll,packAtlas);
            }
            else
            {
                EditorUserBuildSettings.activeBuildTargetChanged = delegate ()
                {
                    if (EditorUserBuildSettings.activeBuildTarget == buildmap[type])
                    {
                        //pack
                        BuildHandle(type, buildOptions, isBuildExe, clearFolder, isBuildAll,packAtlas);
                    }
                };
                if(buildGroupmap.TryGetValue(type,out var group))
                {
                    EditorUserBuildSettings.SwitchActiveBuildTarget(group, buildmap[type]);
                }
                else
                {
                    EditorUserBuildSettings.SwitchActiveBuildTarget(buildmap[type]);
                }
               
            }
        }
        private static void BuildInternal(BuildTarget buildTarget,bool isBuildExe,bool isBuildAll)
        {
            string jstr = File.ReadAllText("Assets/AssetsPackage/config.bytes");
            var obj = JsonHelper.FromJson<BuildInConfig>(jstr);
            int buildVersion = obj.Resver;
            Debug.Log($"开始构建 : {buildTarget}");

            // 命令行参数

            // 构建参数
            string defaultOutputRoot = AssetBundleBuilderHelper.GetDefaultOutputRoot();
            BuildParameters buildParameters = new BuildParameters();
            buildParameters.OutputRoot = defaultOutputRoot;
            buildParameters.BuildTarget = buildTarget;
            buildParameters.PackageName = "DefaultPackage";
            buildParameters.BuildPipeline = isBuildExe? EBuildPipeline.BuiltinBuildPipeline: EBuildPipeline.ScriptableBuildPipeline;
            buildParameters.SBPParameters = new BuildParameters.SBPBuildParameters();
            buildParameters.BuildMode = isBuildExe?EBuildMode.ForceRebuild:EBuildMode.IncrementalBuild;
            buildParameters.PackageVersion = buildVersion.ToString();
            buildParameters.CopyBuildinFileTags = "buildin";
            buildParameters.VerifyBuildingResult = true;
            // buildParameters.EnableAddressable = true;
            buildParameters.CopyBuildinFileOption = isBuildAll?ECopyBuildinFileOption.ClearAndCopyAll:
                ECopyBuildinFileOption.ClearAndCopyByTags;
            // buildParameters.EncryptionServices = new GameEncryption();
            buildParameters.CompressOption = ECompressOption.LZ4;
            buildParameters.DisableWriteTypeTree = true;//禁止写入类型树结构（可以降低包体和内存并提高加载效率）
            buildParameters.IgnoreTypeTreeChanges = false;
            if (buildParameters.BuildPipeline == EBuildPipeline.ScriptableBuildPipeline)
            {
                buildParameters.SBPParameters = new BuildParameters.SBPBuildParameters();
                buildParameters.SBPParameters.WriteLinkXML = true;
            }
            // 执行构建
            AssetBundleBuilder builder = new AssetBundleBuilder();
            var buildResult = builder.Run(buildParameters);
            if (buildResult.Success)
                Debug.Log($"构建成功!");
        }
        public static void HandleAtlas()
        {
            //清除图集
            AtlasHelper.ClearAllAtlas();
            //生成图集
            AtlasHelper.GeneratingAtlas();
        }
        static void BuildHandle(PlatformType type, BuildOptions buildOptions, bool isBuildExe,bool clearFolder,
            bool isBuildAll,bool packAtlas)
        {
            BuildTarget buildTarget = BuildTarget.StandaloneWindows;
            string programName = "TaoTie";
            string exeName = programName;
            string platform = "";
            switch (type)
            {
                case PlatformType.Windows:
                    buildTarget = BuildTarget.StandaloneWindows64;
                    exeName += ".exe";
                    platform = "pc";
                    break;
                case PlatformType.Android:
                    KeystoreSetting();
                    buildTarget = BuildTarget.Android;
                    exeName += ".apk";
                    platform = "android";
                    break;
                case PlatformType.IOS:
                    buildTarget = BuildTarget.iOS;
                    platform = "ios";
                    break;
                case PlatformType.MacOS:
                    buildTarget = BuildTarget.StandaloneOSX;
                    platform = "pc";
                    break;
                case PlatformType.Linux:
                    buildTarget = BuildTarget.StandaloneLinux64;
                    platform = "pc";
                    break;
            }
            
            PackagesManagerEditor.Clear("com.thridparty-moudule.hotreload");//HotReload存在时打包会报错
            //打程序集
            FileHelper.CleanDirectory(Define.HotfixDir);
            BuildAssemblieEditor.BuildCodeRelease();
            
            //处理图集资源
            if(packAtlas) HandleAtlas();
            
            //打ab
            BuildInternal(buildTarget, isBuildExe, isBuildAll);

            if (clearFolder && Directory.Exists(relativeDirPrefix))
            {
                Directory.Delete(relativeDirPrefix, true);
                Directory.CreateDirectory(relativeDirPrefix);
            }
            else
            {
                Directory.CreateDirectory(relativeDirPrefix);
            }

            if (isBuildExe)
            {

                AssetDatabase.Refresh();
                string[] levels = {
                    "Assets/AssetsPackage/Scenes/InitScene/Init.unity",
                };
                UnityEngine.Debug.Log("开始EXE打包");
                BuildPipeline.BuildPlayer(levels, $"{relativeDirPrefix}/{exeName}", buildTarget, buildOptions);
                UnityEngine.Debug.Log("完成exe打包");
                
            }
            
            string jstr = File.ReadAllText("Assets/AssetsPackage/config.bytes");
            var obj = JsonHelper.FromJson<BuildInConfig>(jstr);
            
            string fold = $"{AssetBundleBuilderHelper.GetDefaultOutputRoot()}/{buildTarget}/{obj.Resver}";
            
            string targetPath = Path.Combine(relativeDirPrefix, $"{obj.Channel}_{platform}");
            if (!Directory.Exists(targetPath)) Directory.CreateDirectory(targetPath);
            FileHelper.CleanDirectory(targetPath);
            FileHelper.CopyFiles(fold, targetPath);
            
            UnityEngine.Debug.Log("完成cdn资源打包");
#if UNITY_EDITOR
            Application.OpenURL($"file:///{targetPath}");
#endif
        }
        
    }
}
