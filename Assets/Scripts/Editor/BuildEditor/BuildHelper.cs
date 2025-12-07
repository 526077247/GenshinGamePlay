using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Obfuz.Settings;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
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

        private static string[] ignoreFile = new[] {"BuildReport_", ".report", "link.xml", ".json"};

        public static readonly Dictionary<PlatformType, BuildTarget> buildmap =
            new Dictionary<PlatformType, BuildTarget>()
            {
                {PlatformType.Android, BuildTarget.Android},
                {PlatformType.Windows, BuildTarget.StandaloneWindows64},
                {PlatformType.IOS, BuildTarget.iOS},
                {PlatformType.MacOS, BuildTarget.StandaloneOSX},
                {PlatformType.Linux, BuildTarget.StandaloneLinux64},
                {PlatformType.WebGL, BuildTarget.WebGL},
#if TUANJIE_1_5_OR_NEWER
                {PlatformType.WeChat, BuildTarget.MiniGame},
                {PlatformType.TikTok, BuildTarget.MiniGame},
                {PlatformType.KuaiShou, BuildTarget.MiniGame},
                {PlatformType.Minihost, BuildTarget.MiniGame},
#endif
            };

        public static readonly Dictionary<PlatformType, BuildTargetGroup> buildGroupmap =
            new Dictionary<PlatformType, BuildTargetGroup>()
            {
                {PlatformType.Android, BuildTargetGroup.Android},
                {PlatformType.Windows, BuildTargetGroup.Standalone},
                {PlatformType.IOS, BuildTargetGroup.iOS},
                {PlatformType.MacOS, BuildTargetGroup.Standalone},
                {PlatformType.Linux, BuildTargetGroup.Standalone},
                {PlatformType.WebGL, BuildTargetGroup.WebGL},
#if TUANJIE_1_5_OR_NEWER
                {PlatformType.WeChat, BuildTargetGroup.MiniGame},
                {PlatformType.TikTok, BuildTargetGroup.MiniGame},
                {PlatformType.KuaiShou, BuildTargetGroup.MiniGame},
                {PlatformType.Minihost, BuildTargetGroup.MiniGame},
#endif
            };
#if TUANJIE_1_5_OR_NEWER
        public static readonly Dictionary<PlatformType, string> buildSubGroupmap =
            new Dictionary<PlatformType, string>()
            {
                {PlatformType.WeChat, "WeChat"},
                {PlatformType.TikTok, "DouYin"},
                {PlatformType.KuaiShou, "KuaiShou"},
                {PlatformType.Minihost, "Minihost"},
            };
#endif
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
        public static void Switch(PlatformType type)
        {
            if (buildmap[type] == EditorUserBuildSettings.activeBuildTarget)
            {
#if TUANJIE_1_5_OR_NEWER
                if (buildSubGroupmap.ContainsKey(type) && !PlayerSettings.MiniGame.CheckActiveSubplatform(buildSubGroupmap[type]))
                {
                    PlayerSettings.MiniGame.SetActiveSubplatform(buildSubGroupmap[type], true);
                }
#endif
            }
            else
            {
                EditorUserBuildSettings.activeBuildTargetChanged = delegate()
                {
                    if (EditorUserBuildSettings.activeBuildTarget == buildmap[type])
                    {
#if TUANJIE_1_5_OR_NEWER
                        if (buildSubGroupmap.ContainsKey(type) && !PlayerSettings.MiniGame.CheckActiveSubplatform(buildSubGroupmap[type]))
                        {
                            PlayerSettings.MiniGame.SetActiveSubplatform(buildSubGroupmap[type], true);
                        }
#endif
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
        public static void Build(PlatformType type, BuildOptions buildOptions, bool isBuildExe, bool clearReleaseFolder,
            bool clearABFolder, bool buildHotfixAssembliesAOT, bool isBuildAll, bool packAtlas, bool isContainsAb, 
            string channel, bool buildDll = true, string bgPath = null)
        {
            //pack
            BuildHandle(type, buildOptions, isBuildExe, clearReleaseFolder,clearABFolder, buildHotfixAssembliesAOT, 
                isBuildAll, packAtlas, isContainsAb, channel, buildDll, bgPath);
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
#if TUANJIE_1_5_OR_NEWER
                case PlatformType.WeChat:
                case PlatformType.TikTok:
                case PlatformType.KuaiShou:
                case PlatformType.Minihost:
                    buildTarget = BuildTarget.MiniGame;
                    platform = "webgl";
                    break;
#endif
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
            var rename = config.GetChannel();
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
        private static bool BuildInternal(BuildTarget buildTarget,bool isBuildAll, bool isContainsAb, string channel)
        {
            string jstr = File.ReadAllText("Assets/AssetsPackage/config.bytes");
            var obj = JsonHelper.FromJson<PackageConfig>(jstr);
            int buildVersion = obj.GetPackageMaxVersion(Define.DefaultName);
            Debug.Log($"开始构建 : {buildTarget}");
            bool res = BuildPackage(buildTarget, isBuildAll, buildVersion, Define.DefaultName, channel);
            if (!res) return res;
            if (isContainsAb)
            {
                if (obj.OtherPackageMaxVer != null)
                {
                    foreach (var item in obj.OtherPackageMaxVer)
                    {
                        for (int i = 0; i < item.Value.Length; i++)
                        {
                            if(item.Value[i] == Define.DefaultName) continue;
                            res &= BuildPackage(buildTarget, isBuildAll, item.Key, item.Value[i], channel);
                            if (!res) return res;
                        }
                    }
                }
            }
            return res;
        }

        public static bool BuildPackage(BuildTarget buildTarget, bool isBuildAll, int buildVersion,
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
            return buildResult.Success;
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
            string channel, bool buildDll = true, string bgPath = null)
        {
            string jstr = File.ReadAllText("Assets/AssetsPackage/config.bytes");
            var obj = JsonHelper.FromJson<PackageConfig>(jstr);
            
            var vs = Application.version.Split(".");
            var bundleVersionCode = int.Parse(vs[vs.Length-1]);
            string exeName = programName + "_" + channel;
            string platform = "";
            BuildTarget buildTarget = BuildTarget.StandaloneWindows;
            BuildTargetGroup buildTargetGroup = BuildTargetGroup.Standalone;
            int buildVersion = obj.GetPackageMaxVersion(Define.DefaultName);
            switch (type)
            {
                case PlatformType.Windows:
                    buildTarget = BuildTarget.StandaloneWindows64;
                    buildTargetGroup = BuildTargetGroup.Standalone;
                    exeName += ".exe";
                    platform = "pc";
                    break;
                case PlatformType.Android:
                    KeystoreSetting();
                    PlayerSettings.Android.bundleVersionCode = bundleVersionCode + 1;
                    EditorUserBuildSettings.exportAsGoogleAndroidProject = false;
                    buildTarget = BuildTarget.Android;
                    buildTargetGroup = BuildTargetGroup.Android;
                    exeName += Application.version + ".apk";
                    platform = "android";
                    break;
                case PlatformType.IOS:
                    buildTarget = BuildTarget.iOS;
                    buildTargetGroup = BuildTargetGroup.iOS;
                    platform = "ios";
                    break;
                case PlatformType.MacOS:
                    buildTarget = BuildTarget.StandaloneOSX;
                    buildTargetGroup = BuildTargetGroup.Standalone;
                    platform = "pc";
                    break;
                case PlatformType.Linux:
                    buildTarget = BuildTarget.StandaloneLinux64;
                    buildTargetGroup = BuildTargetGroup.Standalone;
                    platform = "pc";
                    break;
                case PlatformType.WebGL:
                    buildTarget = BuildTarget.WebGL;
                    buildTargetGroup = BuildTargetGroup.WebGL;
                    platform = "webgl";
                    exeName += "_" + buildVersion;
                    break;
#if TUANJIE_1_5_OR_NEWER
                case PlatformType.WeChat:
                case PlatformType.TikTok:
                case PlatformType.KuaiShou:
                case PlatformType.Minihost:
                    buildTarget = BuildTarget.MiniGame;
                    buildTargetGroup = BuildTargetGroup.MiniGame;
                    platform = "webgl";
                    exeName += "_" + buildVersion;
                    break;
#endif
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
                var root = AssetBundleBuilderHelper.GetStreamingAssetsRoot();
                if (Directory.Exists(root))
                {
                    FileHelper.CleanDirectory(root);
                }
                AssetDatabase.Refresh();
            }

            if (clearABFolder)
            {
                string abPath = AssetBundleBuilderHelper.GetDefaultBuildOutputRoot();
                if (Directory.Exists(abPath))
                {
                    FileHelper.CleanDirectory(abPath);
                }
            }
                              
            //打ab
            if (!BuildInternal(buildTarget, isBuildAll, isContainsAb, channel))
            {
                return;
            }

            if (clearReleaseFolder && Directory.Exists(relativeDirPrefix))
            {
                FileHelper.CleanDirectory(relativeDirPrefix);
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
            //         ObfuzSettings settings = ObfuzSettings.Instance;
            //         if(!settings.buildPipelineSettings.enable)
            //         {
            //             HybridCLR.Editor.Commands.PrebuildCommand.GenerateAll();
            //         }
            //         else
            //         {
            //             Obfuz4HybridCLR.PrebuildCommandExt.GenerateAll();
            //         }
            //     }
            // }
            
            var config = Resources.Load<CDNConfig>("CDNConfig");
            var rename = config.GetChannel();
            string targetPath = Path.Combine(relativeDirPrefix, $"{rename}_{platform}");

            if (!Directory.Exists(targetPath)) Directory.CreateDirectory(targetPath);
            FileHelper.CleanDirectory(targetPath);

            if(isBuildExe)
            {
#if UNITY_WEBGL
                bool webgl1 = true;
                if (PlayerSettings.colorSpace == ColorSpace.Linear || PlayerSettings.GetUseDefaultGraphicsAPIs(buildTarget))
                {
                    webgl1 = false;
                }
                else
                {
                    GraphicsDeviceType[] apis = PlayerSettings.GetGraphicsAPIs(BuildTarget.WebGL);
                    for (int i = 0; i < apis.Length; i++)
                    {
                        if (apis[i] == GraphicsDeviceType.OpenGLES3)
                        {
                            webgl1 = false;
                            break;
                        }
                    }
                }

                if (webgl1)
                {
                    var define = "UNITY_WEBGL_1";
                    var definesString = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup).Trim();
                    if (!string.IsNullOrEmpty(definesString))
                    {
                        definesString += definesString.EndsWith(";") ? define+";" : $";{define};";
                    }
                    else
                    {
                        definesString = define+";";
                    }
                    PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, definesString);
                }
#endif
                AssetDatabase.Refresh();
                UnityEngine.Debug.Log("开始打包");
#if MINIGAME_SUBPLATFORM_DOUYIN
                TTSDK.Tool.StarkBuilderSettings setting = TTSDK.Tool.StarkBuilderSettings.Instance;
                if (setting != null)
                {
                    setting.assetBundleFSEnabled = true;
                    setting.isOldBuildFormat = false;
                    setting.webglPackagePath = Path.GetFullPath("Release");
#if !TUANJIE_1_5_OR_NEWER
                    setting.isWebGL2 = !webgl1;
#endif
                }

                TTSDK.Tool.API.BuildManager.Build(TTSDK.Tool.Framework.Wasm);
                UnityEngine.Debug.Log("完成打包");
#elif MINIGAME_SUBPLATFORM_WEXIN
                WeChatWASM.WXConvertCore.config.ProjectConf.relativeDST = Path.GetFullPath("Release");
                WeChatWASM.WXConvertCore.config.ProjectConf.DST = Path.GetFullPath("Release");
                WeChatWASM.WXConvertCore.config.ProjectConf.CDN = $"{config.DefaultHostServer}/{rename}_{platform}/";
                var fields = typeof(CacheKeys).GetFields();
                foreach (var item in fields)
                {
                    if (item.IsStatic)
                    {
                        var val = item.GetValue(null) as string;
                        if (!string.IsNullOrEmpty(val))
                        {
                            if(!WeChatWASM.WXConvertCore.config.PlayerPrefsKeys.Contains(val)) 
                                WeChatWASM.WXConvertCore.config.PlayerPrefsKeys.Add(val);
                        }
                    }
                }
#if !TUANJIE_1_5_OR_NEWER
                WeChatWASM.WXConvertCore.config.CompileOptions.Webgl2 = !webgl1;
#endif
                if (WeChatWASM.WXConvertCore.DoExport() != WeChatWASM.WXConvertCore.WXExportError.SUCCEED)
                {
                    UnityEngine.Debug.LogError("打包失败");
                    return;
                }
                UnityEngine.Debug.Log("完成打包");
#else
#if UNITY_WEBGL
                PlayerSettings.WebGL.template = $"PROJECT:TaoTie";
#endif
                string[] levels = {
                    "Assets/AssetsPackage/Scenes/InitScene/Init.unity",
                };
                BuildPipeline.BuildPlayer(levels, $"{relativeDirPrefix}/{exeName}", buildTarget, buildOptions);
                
                //清下缓存
                if (Directory.Exists(Application.persistentDataPath))
                {
                    Directory.Delete(Application.persistentDataPath, true);
                }
                UnityEngine.Debug.Log("完成打包");
#endif
            }

            PostProcess(buildTarget, obj, targetPath, config, rename, platform, exeName, bgPath);
            
        }

        static void PostProcess(BuildTarget buildTarget, PackageConfig obj, string targetPath, CDNConfig config,
            string rename, string platform, string exeName, string bgPath)
        {
            if (buildTarget == BuildTarget.WebGL)
            {
                var icons = PlayerSettings.GetIconsForTargetGroup(BuildTargetGroup.Unknown);
                if (icons.Length > 0 && icons[0] != null)
                {
                    var path = AssetDatabase.GetAssetPath(icons[0]);
                    File.Copy(path,$"{relativeDirPrefix}/{exeName}/icon.png", true);
                }

                if (!string.IsNullOrEmpty(bgPath))
                {
                    string path = $"{relativeDirPrefix}/{exeName}/Build/{exeName}.jpg";
                    if (!File.Exists(path))
                    {
                        string h5 = $"{relativeDirPrefix}/{exeName}/index.html";
                        var lines = File.ReadAllLines(h5);
                        for (int i = 0; i < lines.Length; i++)
                        {
                            if (lines[i].Contains("background:"))
                            {
                                lines[i] =
                                    $"      background: url('Build/{exeName}.jpg') no-repeat center center;";
                                break;
                            }
                        }
                        File.WriteAllLines(h5, lines);
                    }
                    File.Copy(bgPath, path, true);
                }
            }
#if MINIGAME_SUBPLATFORM_DOUYIN
            var newPath = relativeDirPrefix + "/tt-minigame/";
            //新打包格式
            if (Directory.Exists(newPath))
            {
                if (File.Exists(newPath + "game.js"))
                {
                    var txt = File.ReadAllText(newPath + "game.js");
                    txt = txt.Replace("['正在加载资源']","['正在加载资源','正在加载配置','正在生成世界']");
                    txt = txt.Replace("'编译中'", "'正在编译'");
                    txt = txt.Replace("'初始化中'", "'正在初始化'");
                    txt = txt.Replace("textDuration: 1500,", "textDuration: 6000,");
                    txt = txt.Replace("scaleMode: scaleMode.default,", "scaleMode: scaleMode.noBorder,");
                    txt = txt.Replace("width: 106,", "width: 64,");
                    txt = txt.Replace("height: 40,", "height: 64,");
                    var preload = GetPreloadFileUrls(null, buildTarget, config, rename, platform);
                    if (!string.IsNullOrEmpty(preload))
                    {
                        txt = txt.Replace(
                            "// 'DATA_CDN/StreamingAssets/WebGL/textures_005b9e6b32e22099edc38cba5b3d11de',",
                            preload);
                    }

                    File.WriteAllText(newPath + "game.js", txt);
                }

                var icons = PlayerSettings.GetIconsForTargetGroup(BuildTargetGroup.Unknown);
                if (icons.Length > 0 && icons[0] != null)
                {
                    var path = AssetDatabase.GetAssetPath(icons[0]);
                    File.Copy(path, $"{newPath}/images/unity_logo.png", true);
                }

                //File.Copy("Assets/background.png", $"{newPath}/images/background.png", true);//背景图
                if (File.Exists(newPath + "game.json")) //处理json格式报错
                {
                    var gamejStr = File.ReadAllText(newPath + "game.json");
                    gamejStr = gamejStr.Replace("0.1.0", "4.21.0");
                    var gameInfo = Newtonsoft.Json.JsonConvert.DeserializeObject(gamejStr);
                    gamejStr = Newtonsoft.Json.JsonConvert.SerializeObject(gameInfo);
                    File.WriteAllText(newPath + "game.json", gamejStr);
                }
            }
#elif MINIGAME_SUBPLATFORM_WEXIN
            if (WeChatWASM.WXConvertCore.config.ProjectConf.assetLoadType == 0)
            {
                string[] fls = Directory.GetFiles(WeChatWASM.WXConvertCore.config.ProjectConf.DST +"/webgl");
                for (int i = 0; i < fls.Length; i++)
                {
                    if (fls[i].EndsWith(".data") || fls[i].EndsWith("data.zip") || fls[i].EndsWith("data.br")|| fls[i].EndsWith("bin.txt"))
                    {
                        var name = Path.GetFileName(fls[i]);
                        File.Copy(fls[i], targetPath + "/" + name);
                    }
                }
            }
            var newPath = relativeDirPrefix + "/minigame/";
            if (Directory.Exists(newPath))
            {
                if (File.Exists(newPath + "game.js"))
                {
                    var txt = File.ReadAllText(newPath + "game.js");
                    txt = txt.Replace(
                        $"{YooAssetSettingsData.Setting.DefaultYooFolderName}/{Define.DefaultName}/",
                        WeChatWASM.WXConvertCore.config.ProjectConf.CDN);
                    File.WriteAllText(newPath + "game.js", txt);
                }
            }
#endif
            string fold = $"{AssetBundleBuilderHelper.GetDefaultBuildOutputRoot()}/{buildTarget}";
            
            var dirs = new DirectoryInfo(fold).GetDirectories();
            for (int i = 0; i < dirs.Length; i++)
            {
                var version = obj.GetPackageMaxVersion(dirs[i].Name);
                string dir = $"{fold}/{dirs[i].Name}/{version}";
                if (Directory.Exists(dir))
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
        
        public static void BuildApk(string channel,BuildOptions buildOptions)
        {
            var bundleVersionCode = int.Parse(Application.version.Split(".")[2]);
            string exeName = programName + "_" + channel;
            KeystoreSetting();
            PlayerSettings.Android.bundleVersionCode = bundleVersionCode;
            EditorUserBuildSettings.exportAsGoogleAndroidProject = false;
            BuildTarget buildTarget  = BuildTarget.Android;
            exeName += Application.version + ".apk";
            AssetDatabase.Refresh();
            string[] levels =
            {
                "Assets/AssetsPackage/Scenes/InitScene/Init.unity",
            };
            UnityEngine.Debug.Log("开始打包");
            BuildPipeline.BuildPlayer(levels, $"{relativeDirPrefix}/{exeName}", buildTarget, buildOptions);
            UnityEngine.Debug.Log("完成打包");
        }

        public static void CollectSVC(Action<bool> callBack)
        {
            string savePath = "Assets/AssetsPackage/RenderAssets/ShaderVariants.shadervariants";
            ShaderVariantCollector.Run(savePath, Define.DefaultName, 1000, () =>
            {
                ShaderVariantCollection collection =
                    AssetDatabase.LoadAssetAtPath<ShaderVariantCollection>(savePath);
                if (collection != null)
                {
                    Debug.Log($"ShaderCount : {collection.shaderCount}");
                    Debug.Log($"VariantCount : {collection.variantCount}");
                }
                callBack?.Invoke(collection != null);
            });
        }

        public static string GetPreloadFileUrls(List<string> address, BuildTarget buildTarget, CDNConfig config, 
            string rename, string platform, bool includeBuildins = false)
        {
            if (address == null) return null;
            HashSet<string> hashSet = new HashSet<string>();
            for (int i = 0; i < address.Count; i++)
            {
                hashSet.Add(address[i]);
            }
            var files = new List<string>();
            string fold = $"{AssetBundleBuilderHelper.GetDefaultBuildOutputRoot()}/{buildTarget}";
            FileHelper.GetAllFiles(files, fold);
            BuildReport buildReport = null;
            for (int i = 0; i < files.Count; i++)
            {
                if (files[i].EndsWith(".report"))
                {
                    string jsonData = FileUtility.ReadAllText(files[i]);
                    buildReport = BuildReport.Deserialize(jsonData);
                    break;
                }
            }

            if (buildReport != null)
            {
                HashSet<string> bundles = new HashSet<string>();
                for (int j = 0; j < buildReport.AssetInfos.Count; j++)
                {
                    if (hashSet.Contains(buildReport.AssetInfos[j].Address))
                    {
                        bundles.Add(buildReport.AssetInfos[j].MainBundleName);
                    }
                }

                MultiMap<string, string> maps = new MultiMap<string, string>();
                Dictionary<string, ReportBundleInfo> path = new Dictionary<string, ReportBundleInfo>();
                hashSet.Clear();
                for (int j = 0; j < buildReport.BundleInfos.Count; j++)
                {
                    maps.Add(buildReport.BundleInfos[j].BundleName, buildReport.BundleInfos[j].DependBundles);
                    path.Add(buildReport.BundleInfos[j].BundleName, buildReport.BundleInfos[j]);
                }
                List<string> allBundles = bundles.ToList();
                for (int i = 0; i < allBundles.Count; i++)
                {
                    if (maps.TryGetValue(allBundles[i], out var list))
                    {
                        for (int j = 0; j < list.Count; j++)
                        {
                            if (!allBundles.Contains(list[j]))
                            {
                                allBundles.Add(list[j]);
                            }
                        }
                    }
                }
                
                StringBuilder preloadList = new StringBuilder();
                if (includeBuildins)
                {
                    var streamingAssets = new List<string>();
                    FileHelper.GetAllFiles(streamingAssets, Application.streamingAssetsPath);
                    for (int i = 0; i < streamingAssets.Count; i++)
                    {
                        if(streamingAssets[i].EndsWith(".meta")) continue;
                        preloadList.Append(Path.GetFileName(streamingAssets[i]) + ";");
                    }
                }
                
                foreach (var item in allBundles)
                {
                    if (path.TryGetValue(item, out var p))
                    {
                        if(!p.GetTagsString().Contains("buildin") || !includeBuildins) 
                            preloadList.AppendLine($"'{config.DefaultHostServer}/{rename}_{platform}/{p.FileName}',");
                    }
                }
            
                return preloadList.ToString();
            }

            return null;
        }
    }
}
