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
            };

        public static readonly Dictionary<PlatformType, BuildTargetGroup> buildGroupmap =
            new Dictionary<PlatformType, BuildTargetGroup>(PlatformTypeComparer.Instance)
            {
                {PlatformType.Android, BuildTargetGroup.Android},
                {PlatformType.Windows, BuildTargetGroup.Standalone},
                {PlatformType.IOS, BuildTargetGroup.iOS},
                {PlatformType.MacOS, BuildTargetGroup.Standalone},
                {PlatformType.Linux, BuildTargetGroup.Standalone},
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
        public static void SetCdnConfig(string channel, int mode = 1, string cdnPath = "")
        {
            var cdn = Resources.Load<CDNConfig>("CDNConfig");
            cdn.Channel = channel;

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

        public static void Build(PlatformType type, BuildOptions buildOptions, bool isBuildExe, bool clearFolder,
            bool buildHotfixAssembliesAOT, bool isBuildAll, bool packAtlas, bool isContainsAb, string channel,
            bool buildDll = true)
        {
            if (buildmap[type] == EditorUserBuildSettings.activeBuildTarget)
            {
                //pack
                BuildHandle(type, buildOptions, isBuildExe, clearFolder, buildHotfixAssembliesAOT, isBuildAll,
                    packAtlas, isContainsAb, channel, buildDll);
            }
            else
            {
                EditorUserBuildSettings.activeBuildTargetChanged = delegate()
                {
                    if (EditorUserBuildSettings.activeBuildTarget == buildmap[type])
                    {
                        //pack
                        BuildHandle(type, buildOptions, isBuildExe, clearFolder, buildHotfixAssembliesAOT, isBuildAll,
                            packAtlas, isContainsAb, channel, buildDll);
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
            }

            string jstr = File.ReadAllText("Assets/AssetsPackage/packageConfig.bytes");
            var packageConfig = JsonHelper.FromJson<PackageConfig>(jstr);
            if (!packageConfig.packageVer.ContainsKey(packageName))
            {
                Debug.LogError("指定分包版本号不存在");
                return;
            }

            jstr = File.ReadAllText("Assets/AssetsPackage/config.bytes");
            var obj = JsonHelper.FromJson<BuildInConfig>(jstr);
            int version = obj.Resver;
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
            var obj = JsonHelper.FromJson<BuildInConfig>(jstr);
            int buildVersion = obj.Resver;
            Debug.Log($"开始构建 : {buildTarget}");
            BuildPackage(buildTarget, isBuildAll, buildVersion, YooAssetsMgr.DefaultName, channel);
            jstr = File.ReadAllText("Assets/AssetsPackage/packageConfig.bytes");
            var packageConfig = JsonHelper.FromJson<PackageConfig>(jstr);
            if (isContainsAb)
            {
                if (packageConfig.packageVer != null)
                {
                    foreach (var item in packageConfig.packageVer)
                    {
                        BuildPackage(buildTarget, isBuildAll, item.Value, item.Key, channel);
                    }
                }
            }
        }

        public static void BuildPackage(BuildTarget buildTarget, bool isBuildAll, int buildVersion,
            string packageName, string channel)
        {
            // 构建参数
            string defaultOutputRoot = AssetBundleBuilderHelper.GetDefaultBuildOutputRoot();
            BuildParameters buildParameters = new BuildParameters();
            buildParameters.BuildOutputRoot = defaultOutputRoot;
            buildParameters.BuildTarget = buildTarget;
            buildParameters.PackageName = packageName;
            buildParameters.BuildPipeline = EBuildPipeline.ScriptableBuildPipeline;
            buildParameters.SBPParameters = new BuildParameters.SBPBuildParameters();
            buildParameters.BuildMode = EBuildMode.IncrementalBuild;
            buildParameters.PackageVersion = buildVersion.ToString();
            buildParameters.CopyBuildinFileTags = buildAllChannel.Contains(channel)?"buildin;buildinplus":"buildin;";
            buildParameters.VerifyBuildingResult = true;
            buildParameters.StreamingAssetsRoot = StreamingAssetsDefine.StreamAssetsDir;
            if (packageName == YooAssetsMgr.DefaultName)
            {
                buildParameters.CopyBuildinFileOption = isBuildAll
                    ? ECopyBuildinFileOption.ClearAndCopyAll
                    : ECopyBuildinFileOption.ClearAndCopyByTags;
            }
            else
            {
                buildParameters.CopyBuildinFileOption =
                    isBuildAll ? ECopyBuildinFileOption.OnlyCopyAll : ECopyBuildinFileOption.OnlyCopyByTags;
            }

            buildParameters.EncryptionServices = new StreamEncryption();
            buildParameters.CompressOption = ECompressOption.LZ4;
            buildParameters.DisableWriteTypeTree = true; //禁止写入类型树结构（可以降低包体和内存并提高加载效率）
            buildParameters.IgnoreTypeTreeChanges = false;
            buildParameters.SharedPackRule = new ZeroRedundancySharedPackRule();
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
            else
                Debug.LogError(buildResult.ErrorInfo);
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

        static void BuildHandle(PlatformType type, BuildOptions buildOptions, bool isBuildExe, bool clearFolder,
            bool buildHotfixAssembliesAOT, bool isBuildAll, bool packAtlas, bool isContainsAb, 
            string channel, bool buildDll = true)
        {
            // var scene = EditorSceneManager.OpenScene("Assets/AssetsPackage/Scenes/InitScene/Init.unity");
            // var init = GameObject.Find("Global").GetComponent<Init>();
            // if (init.PlayMode == EPlayMode.EditorSimulateMode)
            // {
            //     init.PlayMode = EPlayMode.HostPlayMode;
            //     init.CodeMode = CodeMode.Wolong;
            //     EditorSceneManager.SaveScene(scene);
            // }
            var bundleVersionCode = int.Parse(Application.version.Split(".")[2]);
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
                    BuildAssemblieEditor.BuildCodeRelease();
                else
                    BuildAssemblieEditor.BuildCodeDebug();
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
                              
            //打ab
            BuildInternal(buildTarget, isBuildAll, isContainsAb, channel);

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
                // if (HybridCLR.Editor.SettingsUtil.Enable)
                // {
                //     HybridCLR.Editor.SettingsUtil.buildHotfixAssembliesAOT = buildHotfixAssembliesAOT;
                //     HybridCLR.Editor.Commands.PrebuildCommand.GenerateAll();
                // } 
 
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
            var obj = JsonHelper.FromJson<BuildInConfig>(jstr);
            
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
            jstr = File.ReadAllText("Assets/AssetsPackage/packageConfig.bytes");
            var packageConfig = JsonHelper.FromJson<PackageConfig>(jstr);
            for (int i = 0; i < dirs.Length; i++)
            {
                string dir = null;
                if (dirs[i].Name == YooAssetsMgr.DefaultName)
                {
                    dir = $"{fold}/{dirs[i].Name}/{obj.Resver}";
                }
                else
                {
                    if (packageConfig.packageVer != null)
                    {
                        foreach (var item in packageConfig.packageVer)
                        {
                            if (item.Key == dirs[i].Name && isContainsAb)
                            {
                                dir = $"{fold}/{dirs[i].Name}/{item.Value}";
                            }
                        }
                    }
                }

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
