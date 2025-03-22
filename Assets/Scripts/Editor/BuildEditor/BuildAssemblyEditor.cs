using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEditor;
using UnityEditor.Compilation;

namespace TaoTie
{
    public static class BuildAssemblyEditor
    {
        private static bool IsBuildCodeAuto;
        [MenuItem("Tools/Build/EnableAutoBuildCodeDebug _F1")]
        public static void SetAutoBuildCode()
        {
            PlayerPrefs.SetInt("AutoBuild", 1);
            ShowNotification("AutoBuildCode Enabled");
        }
        
        [MenuItem("Tools/Build/DisableAutoBuildCodeDebug _F2")]
        public static void CancelAutoBuildCode()
        {
            PlayerPrefs.DeleteKey("AutoBuild");
            ShowNotification("AutoBuildCode Disabled");
        }
        
        [MenuItem("Tools/Build/BuildCodeDebug _F5")]
        public static void BuildCodeDebug()
        {
            string jstr = File.ReadAllText("Assets/AssetsPackage/config.bytes");
            var config = JsonHelper.FromJson<PackageConfig>(jstr);
            string assemblyName = "Code" + config.GetPackageMaxVersion(Define.DefaultName);
            BuildMuteAssembly(assemblyName, new []
            {
                "Assets/Scripts/Code",
            }, Array.Empty<string>(), CodeOptimization.Debug);

            AfterCompiling(assemblyName);
            
        }
        
        [MenuItem("Tools/Build/BuildCodeRelease _F6")]
        public static void BuildCodeRelease()
        {
            string jstr = File.ReadAllText("Assets/AssetsPackage/config.bytes");
            var config = JsonHelper.FromJson<PackageConfig>(jstr);
            string assemblyName = "Code" + config.GetPackageMaxVersion(Define.DefaultName);
            BuildMuteAssembly(assemblyName, new []
            {
                "Assets/Scripts/Code",
            }, Array.Empty<string>(),CodeOptimization.Release);

            AfterCompiling(assemblyName);

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

        private static void AfterCompiling(string assemblyName)
        {
            while (EditorApplication.isCompiling)
            {
                Debug.Log("Compiling wait1");
                // 主线程sleep并不影响编译线程
                Thread.Sleep(1000);
                Debug.Log("Compiling wait2");
            }
            AfterBuild(assemblyName);
            //反射获取当前Game视图，提示编译完成
            // ShowNotification("Build Code Success");
        }
        
        public static void AfterBuild(string assemblyName)
        {
            Debug.Log("Compiling finish");
            Directory.CreateDirectory(Define.HotfixDir);
            FileHelper.CleanDirectory(Define.HotfixDir);
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
    }
    
}