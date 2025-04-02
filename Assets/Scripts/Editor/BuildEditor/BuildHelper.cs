using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using YooAsset.Editor;
using YooAsset;

namespace TaoTie
{
    public static class BuildHelper
    {
        const string programName = "TaoTie";
        /// <summary>
        /// 需要打全量首包的
        /// </summary>
        private static HashSet<string> buildAllChannel = new HashSet<string>() {};

        const string relativeDirPrefix = "Release";

        private static string[] ignoreFile = new[] {"BuildReport_"};

        public static readonly Dictionary<PlatformType, BuildTarget> buildmap =
            new Dictionary<PlatformType, BuildTarget>(PlatformTypeComparer.Instance)
            {
                {PlatformType.Android, BuildTarget.Android},
                {PlatformType.Windows, BuildTarget.StandaloneWindows64},
                {PlatformType.IOS, BuildTarget.iOS},
                {PlatformType.MacOS, BuildTarget.StandaloneOSX},
                {PlatformType.Linux, BuildTarget.StandaloneLinux64},
                {PlatformType.WebGL, BuildTarget.WebGL},
            };

        public static readonly Dictionary<PlatformType, BuildTargetGroup> buildGroupmap =
            new Dictionary<PlatformType, BuildTargetGroup>(PlatformTypeComparer.Instance)
            {
                {PlatformType.Android, BuildTargetGroup.Android},
                {PlatformType.Windows, BuildTargetGroup.Standalone},
                {PlatformType.IOS, BuildTargetGroup.iOS},
                {PlatformType.MacOS, BuildTargetGroup.Standalone},
                {PlatformType.Linux, BuildTargetGroup.Standalone},
                {PlatformType.WebGL, BuildTargetGroup.WebGL},
            };

        public static void KeystoreSetting()
        {
            PlayerSettings.Android.keystoreName = "TaoTie.keystore";
            PlayerSettings.Android.keyaliasName = "taitie";
            PlayerSettings.keyaliasPass = "123456";
            PlayerSettings.keystorePass = "123456";
        }

        private static string[] cdnList =
        {
            "http://127.0.0.1:8081/cdn",
            "http://127.0.0.1:8081/cdn",
            "http://127.0.0.1:8081/cdn", 
            "http://127.0.0.1:8081/cdn"
        };
        private static string[] cdnList2 =
        {
            "http://127.0.0.1:8081/cdn",
            "http://127.0.0.1:8081/cdn",
            "http://127.0.0.1:8081/cdn", 
            "http://127.0.0.1:8081/cdn"
        };
        private static string[] cdnTestList =
        {
            "http://127.0.0.1:8081/cdn",
            "http://127.0.0.1:8081/cdn",
            "http://127.0.0.1:8081/cdn", 
            "http://127.0.0.1:8081/cdn"
        };
        /// <summary>
        /// 设置打包模式
        /// </summary>
        public static void SetCdnConfig(string channel,bool buildHotfixAssembliesAOT, int mode = 1, string cdnPath = "")
        {
            var cdn = Resources.Load<CDNConfig>("CDNConfig");
            cdn.Channel = channel;
            cdn.BuildHotfixAssembliesAOT = buildHotfixAssembliesAOT;
            
            if (mode == (int) Mode.自定义服务器)
            {
                cdn.DefaultHostServer = cdnPath;
                cdn.FallbackHostServer = cdnPath;
                cdn.UpdateListUrl = cdnPath;
                cdn.TestUpdateListUrl = cdnPath;
            }
            else
            {
                cdn.DefaultHostServer = cdnList[mode];
                cdn.FallbackHostServer = cdnList2[mode];
                cdn.UpdateListUrl = cdnList[mode];
                cdn.TestUpdateListUrl = cdnTestList[mode];
            }
            EditorUtility.SetDirty(cdn);
            AssetDatabase.SaveAssetIfDirty(cdn);
        }

        public static void Build(PlatformType type, BuildOptions buildOptions, bool isBuildExe, bool clearReleaseFolder,
            bool clearABFolder, bool buildHotfixAssembliesAOT, bool isBuildAll, bool packAtlas, bool isContainsAb, 
            string channel, bool buildDll = true)
        {
            if (buildmap[type] == EditorUserBuildSettings.activeBuildTarget)
            {
                //pack
                BuildHandle(type, buildOptions, isBuildExe, clearReleaseFolder,clearABFolder, buildHotfixAssembliesAOT, 
                    isBuildAll, packAtlas, isContainsAb, channel, buildDll);
            }
            else
            {
                EditorUserBuildSettings.activeBuildTargetChanged = delegate()
                {
                    if (EditorUserBuildSettings.activeBuildTarget == buildmap[type])
                    {
                        //pack
                        BuildHandle(type, buildOptions, isBuildExe, clearReleaseFolder,clearABFolder, buildHotfixAssembliesAOT, 
                            isBuildAll, packAtlas, isContainsAb, channel, buildDll);
                    }
                };
                if (buildGroupmap.TryGetValue(type, out var group))
                {
                    EditorUserBuildSettings.SwitchActiveBuildTarget(group, buildmap[type]);
                }
                else
                {
                    EditorUserBuildSettings.SwitchActiveBuildTarget(buildmap[type]);
                }

            }
        }
        public static void BuildPackage(PlatformType type, string packageName)
        {
            string platform = "";
            BuildTarget buildTarget = BuildTarget.StandaloneWindows;
            switch (type)
            {
                case PlatformType.Windows:
                    buildTarget = BuildTarget.StandaloneWindows64;
 
                    platform = "pc";
                    break;
                case PlatformType.Android:
                    KeystoreSetting();
                    buildTarget = BuildTarget.Android;
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
                case PlatformType.WebGL:
                    buildTarget = BuildTarget.WebGL;
                    platform = "webgl";
                    break;
            }

            string jstr = File.ReadAllText("Assets/AssetsPackage/config.bytes");
            var obj = JsonHelper.FromJson<PackageConfig>(jstr);
            int version = obj.GetPackageMaxVersion(packageName);
            if (version<0)
            {
                Debug.LogError("指定分包版本号不存在");
                return;
            }
            if (buildmap[type] == EditorUserBuildSettings.activeBuildTarget)
            {
                //pack
                BuildPackage(buildTarget, false, version, packageName,null);
            }
            else
            {
                EditorUserBuildSettings.activeBuildTargetChanged = delegate()
                {
                    if (EditorUserBuildSettings.activeBuildTarget == buildmap[type])
                    {
                        //pack
                        BuildPackage(buildTarget, false, version, packageName,null);
                    }
                };
                if (buildGroupmap.TryGetValue(type, out var group))
                {
                    EditorUserBuildSettings.SwitchActiveBuildTarget(group, buildmap[type]);
                }
                else
                {
                    EditorUserBuildSettings.SwitchActiveBuildTarget(buildmap[type]);
                }

            }

            string fold = $"{AssetBundleBuilderHelper.GetDefaultBuildOutputRoot()}/{buildTarget}";
            var config = Resources.Load<CDNConfig>("CDNConfig");
            var rename = "common";
            for (int i = 0; i < Define.RenameList.Length; i++)
            {
                if (Define.RenameList[i] == config.Channel)
                {
                    rename = config.Channel;
                    break;
                }
            }
            string targetPath = Path.Combine(relativeDirPrefix, $"{rename}_{platform}");
            if (!Directory.Exists(targetPath)) Directory.CreateDirectory(targetPath);
            FileHelper.CleanDirectory(targetPath);
            var dir = $"{fold}/{packageName}/{version}";
            FileHelper.CopyFiles(dir, targetPath, ignoreFile);
            UnityEngine.Debug.Log("完成cdn资源打包");
#if UNITY_EDITOR
            Application.OpenURL($"file:///{targetPath}");
#endif
        }
        private static void BuildInternal(BuildTarget buildTarget,bool isBuildAll, bool isContainsAb, string channel)
        {
            string jstr = File.ReadAllText("Assets/AssetsPackage/config.bytes");
            var obj = JsonHelper.FromJson<PackageConfig>(jstr);
            int buildVersion = obj.GetPackageMaxVersion(Define.DefaultName);
            Debug.Log($"开始构建 : {buildTarget}");
            BuildPackage(buildTarget, isBuildAll, buildVersion, Define.DefaultName, channel);
            if (isContainsAb)
            {
                if (obj.OtherPackageMaxVer != null)
                {
                    foreach (var item in obj.OtherPackageMaxVer)
                    {
                        for (int i = 0; i < item.Value.Length; i++)
                        {
                            if(item.Value[i] == Define.DefaultName) continue;
                            BuildPackage(buildTarget, isBuildAll, item.Key, item.Value[i], channel);
                        }
                    }
                }
            }
        }

        public static void BuildPackage(BuildTarget buildTarget, bool isBuildAll, int buildVersion,
            string packageName, string channel)
        {
            var buildoutputRoot = AssetBundleBuilderHelper.GetDefaultBuildOutputRoot();
            var streamingAssetsRoot = AssetBundleBuilderHelper.GetStreamingAssetsRoot();

            var buildParameters = new ScriptableBuildParameters();
            buildParameters.BuildOutputRoot = buildoutputRoot;
            buildParameters.BuildinFileRoot = streamingAssetsRoot;
            buildParameters.BuildTarget = buildTarget;
            buildParameters.PackageName = packageName;
            buildParameters.BuildPipeline = EBuildPipeline.ScriptableBuildPipeline.ToString();
            buildParameters.BuildBundleType = (int)EBuildBundleType.AssetBundle; //必须指定资源包类型
            buildParameters.PackageVersion = buildVersion.ToString();
            buildParameters.BuildinFileCopyParams = buildAllChannel.Contains(channel)?"buildin;buildinplus":"buildin;";
            buildParameters.VerifyBuildingResult = true;
            if (packageName == Define.DefaultName)
            {
                buildParameters.BuildinFileCopyOption = isBuildAll
                    ? EBuildinFileCopyOption.ClearAndCopyAll
                    : EBuildinFileCopyOption.ClearAndCopyByTags;
            }
            else
            {
                buildParameters.BuildinFileCopyOption =
                    isBuildAll ? EBuildinFileCopyOption.OnlyCopyAll : EBuildinFileCopyOption.OnlyCopyByTags;
            }

            buildParameters.EncryptionServices = new FileStreamEncryption();
            buildParameters.CompressOption = ECompressOption.LZ4;
            buildParameters.FileNameStyle = EFileNameStyle.HashName;
            buildParameters.DisableWriteTypeTree = true; //禁止写入类型树结构（可以降低包体和内存并提高加载效率）
            buildParameters.IgnoreTypeTreeChanges = false;
            buildParameters.EnableSharePackRule = true;
            buildParameters.SingleReferencedPackAlone = true;
            buildParameters.WriteLinkXML = true;
            buildParameters.BuiltinShadersBundleName = GetBuiltinShaderBundleName(packageName);
            buildParameters.ClearBuildCacheFiles = false; //不清理构建缓存，启用增量构建，可以提高打包速度！
            buildParameters.UseAssetDependencyDB = true; //使用资源依赖关系数据库，可以提高打包速度！
            // 执行构建
            ScriptableBuildPipeline builder = new ScriptableBuildPipeline();
            var buildResult = builder.Run(buildParameters,true);
            if (buildResult.Success)
                Debug.Log($"构建成功!");
            else
                Debug.LogError(buildResult.ErrorInfo);
        }
        /// <summary>
        /// 内置着色器资源包名称
        /// 注意：和自动收集的着色器资源包名保持一致！
        /// </summary>
        private static string GetBuiltinShaderBundleName(string packageName)
        {
            var uniqueBundleName = AssetBundleCollectorSettingData.Setting.UniqueBundleName;
            var packRuleResult = DefaultPackRule.CreateShadersPackRuleResult();
            return packRuleResult.GetBundleName(packageName, uniqueBundleName);
        }
        public static void HandleAtlas()
        {
            //清除图集
            AtlasHelper.ClearAllAtlas();
            //设置图片
            AtlasHelper.SettingPNG();
            //生成图集
            AtlasHelper.GeneratingAtlas();
        }

        static void BuildHandle(PlatformType type, BuildOptions buildOptions, bool isBuildExe, bool clearReleaseFolder,
            bool clearABFolder, bool buildHotfixAssembliesAOT, bool isBuildAll, bool packAtlas, bool isContainsAb, 
            string channel, bool buildDll = true)
        {
            var vs = Application.version.Split(".");
            var bundleVersionCode = int.Parse(vs[vs.Length-1]);
            string exeName = programName + "_" + channel;
            string platform = "";
            BuildTarget buildTarget = BuildTarget.StandaloneWindows;
            switch (type)
            {
                case PlatformType.Windows:
                    buildTarget = BuildTarget.StandaloneWindows64;
                    exeName += ".exe";
                    platform = "pc";
                    break;
                case PlatformType.Android:
                    KeystoreSetting();
                    PlayerSettings.Android.bundleVersionCode = bundleVersionCode;
                    EditorUserBuildSettings.exportAsGoogleAndroidProject = false;
                    buildTarget = BuildTarget.Android;
                    exeName += Application.version + ".apk";
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
                case PlatformType.WebGL:
                    buildTarget = BuildTarget.WebGL;
                    platform = "webgl";
                    break;
            }

            PackagesManagerEditor.Clear("com.thridparty-moudule.hotreload"); //HotReload存在时打包会报错
            if ((buildOptions & BuildOptions.Development) == 0)
            {
                PackagesManagerEditor.Clear("com.thridparty-moudule.srdebugger"); //正式包去掉srdebugger
            }
            AssetDatabase.RefreshSettings();
            if (buildDll)
            {
                //打程序集
                FileHelper.CleanDirectory(Define.HotfixDir);
                if ((buildOptions & BuildOptions.Development) == 0)
                    BuildAssemblyEditor.BuildCodeRelease();
                else
                    BuildAssemblyEditor.BuildCodeDebug();
            }
            
            AssetDatabase.SaveAssets();
            //处理图集资源
            if (packAtlas) HandleAtlas();
            
            if (isBuildExe)
            {
                if (Directory.Exists("Assets/StreamingAssets"))
                {
                    Directory.Delete("Assets/StreamingAssets", true);
                    Directory.CreateDirectory("Assets/StreamingAssets");
                }
                else
                {
                    Directory.CreateDirectory("Assets/StreamingAssets");
                }
                AssetDatabase.Refresh();
            }

            if (clearABFolder)
            {
                string abPath = AssetBundleBuilderHelper.GetDefaultBuildOutputRoot();
                if (Directory.Exists(abPath))
                {
                    Directory.Delete(relativeDirPrefix, true);
                    Directory.CreateDirectory(relativeDirPrefix);
                }
            }
                              
            //打ab
            BuildInternal(buildTarget, isBuildAll, isContainsAb, channel);

            if (clearReleaseFolder && Directory.Exists(relativeDirPrefix))
            {
                Directory.Delete(relativeDirPrefix, true);
                Directory.CreateDirectory(relativeDirPrefix);
            }
            else
            {
                Directory.CreateDirectory(relativeDirPrefix);
            }

            // if (isBuildExe || buildTarget == BuildTarget.WebGL)
            // {
            //     if (HybridCLR.Editor.SettingsUtil.Enable)
            //     {
            //         HybridCLR.Editor.SettingsUtil.buildHotfixAssembliesAOT = buildHotfixAssembliesAOT;
            //         HybridCLR.Editor.Commands.PrebuildCommand.GenerateAll();
            //     }
            // }

            if(isBuildExe)
            {
                AssetDatabase.Refresh();
                string[] levels = {
                    "Assets/AssetsPackage/Scenes/InitScene/Init.unity",
                };
                UnityEngine.Debug.Log("开始EXE打包");
                BuildPipeline.BuildPlayer(levels, $"{relativeDirPrefix}/{exeName}", buildTarget, buildOptions);
                UnityEngine.Debug.Log("完成exe打包");
                //清下缓存
                Debug.Log(Application.persistentDataPath);
                if (Directory.Exists(Application.persistentDataPath))
                {
                    Directory.Delete(Application.persistentDataPath, true);
                }
            }
            
            string jstr = File.ReadAllText("Assets/AssetsPackage/config.bytes");
            var obj = JsonHelper.FromJson<PackageConfig>(jstr);
            
            string fold = $"{AssetBundleBuilderHelper.GetDefaultBuildOutputRoot()}/{buildTarget}";
            var config = Resources.Load<CDNConfig>("CDNConfig");
            var rename = "common";
            for (int i = 0; i < Define.RenameList.Length; i++)
            {
                if (Define.RenameList[i] == config.Channel)
                {
                    rename = config.Channel;
                    break;
                }
            }
            string targetPath = Path.Combine(relativeDirPrefix, $"{rename}_{platform}");

            if (!Directory.Exists(targetPath)) Directory.CreateDirectory(targetPath);
            FileHelper.CleanDirectory(targetPath);
            var dirs = new DirectoryInfo(fold).GetDirectories();
            for (int i = 0; i < dirs.Length; i++)
            {
                var version = obj.GetPackageMaxVersion(dirs[i].Name);
                string dir = $"{fold}/{dirs[i].Name}/{version}";
                if (dir != null)
                {
                    FileHelper.CopyFiles(dir, targetPath, ignoreFile);
                }
            }

            DirectoryInfo info = new DirectoryInfo(targetPath);
            StringBuilder sb = new StringBuilder();
            var files = info.GetFiles();
            for (int i = 0; i < files.Length; i++)
            {
                sb.AppendLine(files[i].Name);
            }
            File.WriteAllText(relativeDirPrefix + "/reslist.txt",sb.ToString());
            UnityEngine.Debug.Log("完成cdn资源打包");
#if UNITY_EDITOR
            Application.OpenURL($"file:///{targetPath}");
#endif
        }

        public static void PrintFile()
        {
            var config = Resources.Load<CDNConfig>("CDNConfig");
            var rename = "common";
            for (int i = 0; i < Define.RenameList.Length; i++)
            {
                if (Define.RenameList[i] == config.Channel)
                {
                    rename = config.Channel;
                    break;
                }
            }

            string platform = "pc";
            #if UNITY_ANDROID
            platform = "android";
            #elif UNITY_IOS
            platform = "ios";
            #endif
            string targetPath = Path.Combine(relativeDirPrefix, $"{rename}_{platform}");
            DirectoryInfo info = new DirectoryInfo(targetPath);
            if(!info.Exists) return;
            StringBuilder sb = new StringBuilder();
            var files = info.GetFiles();
            for (int i = 0; i < files.Length; i++)
            {
                sb.AppendLine(files[i].Name);
            }
            File.WriteAllText(relativeDirPrefix + "/reslist.txt",sb.ToString());
        }
        public static void BuildApk(string channel,BuildOptions buildOptions)
        {
            var bundleVersionCode = int.Parse(Application.version.Split(".")[2]);
            string exeName = programName + "_" + channel;
            string platform = "";
            KeystoreSetting();
            PlayerSettings.Android.bundleVersionCode = bundleVersionCode;
            EditorUserBuildSettings.exportAsGoogleAndroidProject = false;
            BuildTarget buildTarget  = BuildTarget.Android;
            exeName += Application.version + ".apk";
            platform = "android";

            AssetDatabase.Refresh();
            string[] levels =
            {
                "Assets/AssetsPackage/Scenes/InitScene/Init.unity",
            };
            UnityEngine.Debug.Log("开始EXE打包");
            BuildPipeline.BuildPlayer(levels, $"{relativeDirPrefix}/{exeName}", buildTarget, buildOptions);
            UnityEngine.Debug.Log("完成exe打包");
        }

    }
}
