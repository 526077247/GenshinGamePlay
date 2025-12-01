using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Obfuz;
using Obfuz.Settings;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Player;
using UnityEditor.Compilation;

namespace TaoTie
{
    public static class BuildAssemblyEditor
    {
        private static bool IsBuildCodeAuto;
        [MenuItem("Tools/Build/EnableAutoBuildCodeDebug _F1")]
        public static void SetAutoBuildCode()
        {
            EditorPrefs.SetInt("AutoBuild", 1);
            ShowNotification("AutoBuildCode Enabled");
        }
        
        [MenuItem("Tools/Build/DisableAutoBuildCodeDebug _F2")]
        public static void CancelAutoBuildCode()
        {
            EditorPrefs.DeleteKey("AutoBuild");
            ShowNotification("AutoBuildCode Disabled");
        }
        
        [MenuItem("Tools/Build/CheckCodes _F4")]
        public static void CheckCodes()
        {
            string jstr = File.ReadAllText("Assets/AssetsPackage/config.bytes");
            var config = JsonHelper.FromJson<PackageConfig>(jstr);
            string assemblyName = "Code" + config.GetPackageMaxVersion(Define.DefaultName);
            BuildMuteAssembly(assemblyName, new []
            {
                "Assets/Scripts/Code",
            }, Array.Empty<string>(), CodeOptimization.Debug);
        }
        
        [MenuItem("Tools/Build/BuildCodeDebug _F5")]
        public static void BuildCodeDebug()
        {
            string jstr = File.ReadAllText("Assets/AssetsPackage/config.bytes");
            var config = JsonHelper.FromJson<PackageConfig>(jstr);
            string assemblyName = "Code" + config.GetPackageMaxVersion(Define.DefaultName);
            if (Define.ConfigType == 1)
            {
                BuildMuteAssembly(assemblyName, "Unity.Code", CodeOptimization.Debug);
            }
            else
            {
                BuildMuteAssembly(assemblyName, new []
                {
                    "Assets/Scripts/Code",
                }, Array.Empty<string>(), CodeOptimization.Debug);
            }
            AfterCompiling(assemblyName);
            
        }
        
        [MenuItem("Tools/Build/BuildCodeRelease _F6")]
        public static void BuildCodeRelease()
        {
            string jstr = File.ReadAllText("Assets/AssetsPackage/config.bytes");
            var config = JsonHelper.FromJson<PackageConfig>(jstr);
            string assemblyName = "Code" + config.GetPackageMaxVersion(Define.DefaultName);
            if (Define.ConfigType == 1)
            {
                BuildMuteAssembly(assemblyName, "Unity.Code", CodeOptimization.Release);
            }
            else
            {
                BuildMuteAssembly(assemblyName, new []
                {
                    "Assets/Scripts/Code",
                }, Array.Empty<string>(), CodeOptimization.Release);
            }
            AfterCompiling(assemblyName, true);

        }

        private static void BuildMuteAssembly(string assemblyName, string[] CodeDirectorys, string[] additionalReferences, CodeOptimization codeOptimization,bool isAuto = false)
        {
            List<string> scripts = new List<string>();
            for (int i = 0; i < CodeDirectorys.Length; i++)
            {
                DirectoryInfo dti = new DirectoryInfo(CodeDirectorys[i]);
                FileInfo[] fileInfos = dti.GetFiles("*.cs", System.IO.SearchOption.AllDirectories);
                for (int j = 0; j < fileInfos.Length; j++)
                {
                    scripts.Add(fileInfos[j].FullName);
                }
            }
            if (!Directory.Exists(Define.BuildOutputDir))
                Directory.CreateDirectory(Define.BuildOutputDir);

            string dllPath = Path.Combine(Define.BuildOutputDir, $"{assemblyName}.dll");
            string pdbPath = Path.Combine(Define.BuildOutputDir, $"{assemblyName}.pdb");
            File.Delete(dllPath);
            File.Delete(pdbPath);

            AssemblyBuilder assemblyBuilder = new AssemblyBuilder(dllPath, scripts.ToArray());
            
            //启用UnSafe
            //assemblyBuilder.compilerOptions.AllowUnsafeCode = true;

            BuildTargetGroup buildTargetGroup = BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget);

            assemblyBuilder.compilerOptions.CodeOptimization = codeOptimization;
            assemblyBuilder.compilerOptions.ApiCompatibilityLevel = PlayerSettings.GetApiCompatibilityLevel(buildTargetGroup);
            // assemblyBuilder.compilerOptions.ApiCompatibilityLevel = ApiCompatibilityLevel.NET_4_6;

            assemblyBuilder.additionalReferences = additionalReferences;

            assemblyBuilder.flags = AssemblyBuilderFlags.None;
            //AssemblyBuilderFlags.None                 正常发布
            //AssemblyBuilderFlags.DevelopmentBuild     开发模式打包
            //AssemblyBuilderFlags.EditorAssembly       编辑器状态
            assemblyBuilder.referencesOptions = ReferencesOptions.UseEngineModules;

            assemblyBuilder.buildTarget = EditorUserBuildSettings.activeBuildTarget;

            assemblyBuilder.buildTargetGroup = buildTargetGroup;

            assemblyBuilder.buildStarted += delegate(string assemblyPath) { Debug.LogFormat("build start：" + assemblyPath); };

            assemblyBuilder.buildFinished += delegate(string assemblyPath, CompilerMessage[] compilerMessages)
            {
                IsBuildCodeAuto = false;
                int errorCount = compilerMessages.Count(m => m.type == CompilerMessageType.Error);
                int warningCount = compilerMessages.Count(m => m.type == CompilerMessageType.Warning&&!m.message.Contains("CS0436"));

                Debug.LogFormat("Warnings: {0} - Errors: {1}", warningCount, errorCount);

                if (warningCount > 0)
                {
                    Debug.LogFormat("有{0}个Warning!!!", warningCount);
                }

                if (errorCount > 0||warningCount > 0)
                {
                    for (int i = 0; i < compilerMessages.Length; i++)
                    {
                        if (compilerMessages[i].type == CompilerMessageType.Error ||
                            compilerMessages[i].type == CompilerMessageType.Warning)
                        {
                            if (!compilerMessages[i].message.Contains("CS0436")
                                && !compilerMessages[i].message.Contains("CS0618"))
                                Debug.LogError(compilerMessages[i].message);
                            else
                                Debug.LogWarning(compilerMessages[i].message);

                        }
                    }
                }
            };
            if (isAuto)
            {
                IsBuildCodeAuto = true;
                EditorApplication.CallbackFunction Update = null;
                Update = () =>
                {
                    if(IsBuildCodeAuto||EditorApplication.isCompiling) return;
                    EditorApplication.update -= Update;
                    AfterBuild(assemblyName);
                };
                EditorApplication.update += Update;
            }
            //开始构建
            if (!assemblyBuilder.Build())
            {
                Debug.LogErrorFormat("build fail：" + assemblyBuilder.assemblyPath);
                return;
            }
        }
        
        private static void BuildMuteAssembly(string assemblyName, string asmdefName, CodeOptimization codeOptimization,bool isAuto = false)
        {
            var target = EditorUserBuildSettings.activeBuildTarget;
            var group = BuildPipeline.GetBuildTargetGroup(target);

            ScriptCompilationSettings scriptCompilationSettings = new ScriptCompilationSettings();
            scriptCompilationSettings.group = group;
            scriptCompilationSettings.target = target;
            scriptCompilationSettings.options = codeOptimization == CodeOptimization.Release
                ? ScriptCompilationOptions.None
                : ScriptCompilationOptions.DevelopmentBuild;
            
            if (isAuto)
            {
                IsBuildCodeAuto = true;
                EditorApplication.CallbackFunction Update = null;
                Update = () =>
                {
                    if(IsBuildCodeAuto||EditorApplication.isCompiling) return;
                    EditorApplication.update -= Update;
                    AfterBuild(assemblyName);
                };
                EditorApplication.update += Update;
            }
            
            ScriptCompilationResult scriptCompilationResult = PlayerBuildInterface.CompilePlayerScripts(scriptCompilationSettings, Define.BuildOutputDir);
#if UNITY_2022
            UnityEditor.EditorUtility.ClearProgressBar();
#endif
            File.Move(Path.Combine(Define.BuildOutputDir , $"{asmdefName}.dll"), Path.Combine(Define.BuildOutputDir, $"{assemblyName}.dll"));
            File.Move(Path.Combine(Define.BuildOutputDir , $"{asmdefName}.pdb"), Path.Combine(Define.BuildOutputDir, $"{assemblyName}.pdb"));
            AfterCompiling(assemblyName);
        }

        private static void AfterCompiling(string assemblyName, bool obfuscate = false)
        {
            while (!File.Exists(Path.Combine(Define.BuildOutputDir, $"{assemblyName}.dll"))
                   && EditorApplication.isCompiling)
            {
                Debug.Log("Compiling wait1");
                // 主线程sleep并不影响编译线程
                Thread.Sleep(1000);
                Debug.Log("Compiling wait2");
            }
            AfterBuild(assemblyName, obfuscate);
            //反射获取当前Game视图，提示编译完成
            // ShowNotification("Build Code Success");
            Debug.Log("Build Code Success");
        }
        
        public static void AfterBuild(string assemblyName, bool obfuscate = false)
        {
            Debug.Log("Compiling finish");
            Directory.CreateDirectory(Define.HotfixDir);
            FileHelper.CleanDirectory(Define.HotfixDir);
            if (obfuscate)
            {
                RunObfuscate(assemblyName);
            }
            File.Copy(Path.Combine(Define.BuildOutputDir, $"{assemblyName}.dll"), Path.Combine(Define.HotfixDir, $"{assemblyName}.dll.bytes"), true);
            File.Copy(Path.Combine(Define.BuildOutputDir, $"{assemblyName}.pdb"), Path.Combine(Define.HotfixDir, $"{assemblyName}.pdb.bytes"), true);
            AssetDatabase.Refresh();

            Debug.Log("build success!");
        }

        public static void ShowNotification(string tips)
        {
            var game = EditorWindow.GetWindow(typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView"));
            game?.ShowNotification(new GUIContent($"{tips}"));
        }


        public static string GetStripAssembliesDir2021()
        {
#if UNITY_STANDALONE_WIN
            return $"./Library/Bee/artifacts/WinPlayerBuildProgram/ManagedStripped";
#elif UNITY_ANDROID
            return $"./Library/Bee/artifacts/Android/ManagedStripped";
#elif UNITY_IOS
            return $"./Temp/StagingArea/Data/Managed/tempStrip";
#elif UNITY_WEBGL
            return $"./Library/Bee/artifacts/WebGL/ManagedStripped";
#elif UNITY_EDITOR_OSX
            return $"./Library/Bee/artifacts/MacStandalonePlayerBuildProgram/ManagedStripped";
#else
            throw new NotSupportedException("GetOriginBuildStripAssembliesDir");
#endif
        }

        private static void RunObfuscate(string assemblyName)
        {
            ObfuzSettings settings = ObfuzSettings.Instance;
            if (!settings.buildPipelineSettings.enable)
            {
                Debug.Log("Obfuscation is disabled.");
                return;
            }
            var old = settings.assemblySettings.assembliesToObfuscate;
            settings.assemblySettings.assembliesToObfuscate = new string[] {assemblyName};
            var oldDyn = settings.secretSettings.assembliesUsingDynamicSecretKeys;
            settings.secretSettings.assembliesUsingDynamicSecretKeys = settings.assemblySettings.assembliesToObfuscate;
            Debug.Log("Obfuscation begin...");
            var buildTarget = EditorUserBuildSettings.activeBuildTarget;
            
            var obfuscationRelativeAssemblyNames = new HashSet<string>(settings.assemblySettings.GetObfuscationRelativeAssemblyNames());

            var obfuscatorBuilder = ObfuscatorBuilder.FromObfuzSettings(settings, buildTarget, true, true);

            var assemblySearchDirs = new List<string>
                {
                    Define.BuildOutputDir,
                    "./Library/ScriptAssemblies",
                    GetStripAssembliesDir2021(),
                    "./Assets/Scripts/ThirdParty/Nino",
                };
            obfuscatorBuilder.InsertTopPriorityAssemblySearchPaths(assemblySearchDirs);

            bool succ = false;

            try
            {
                Obfuscator obfuz = obfuscatorBuilder.Build();
                obfuz.Run();

                foreach (var dllName in obfuscationRelativeAssemblyNames)
                {
                    string src = $"{obfuscatorBuilder.CoreSettingsFacade.obfuscatedAssemblyOutputPath}/{dllName}.dll";
                    string dst = Path.Combine(Define.BuildOutputDir,  $"{dllName}.dll");
  
                    if (!File.Exists(src))
                    {
                        Debug.LogWarning($"obfuscation assembly not found! skip copy. path:{src}");
                        continue;
                    }
                    File.Copy(src, dst, true);
                    Debug.Log($"obfuscate dll:{dst}");
                }
                succ = true;
            }
            catch (Exception e)
            {
                succ = false;
                Debug.LogException(e);
                Debug.LogError($"Obfuscation failed.");
            }

            settings.secretSettings.assembliesUsingDynamicSecretKeys = oldDyn;
            settings.assemblySettings.assembliesToObfuscate = old;
            Debug.Log("Obfuscation end.");
        }
        
    }
    
}