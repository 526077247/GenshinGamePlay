/*
 * Author: Misaka Mikoto
 * email: easy66@live.com
 * github: https://github.com/Misaka-Mikoto-Tech/UnityScriptHotReload
 */
#if UNITY_2021_2_OR_NEWER
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using System.Linq;

namespace ScriptHotReload
{
    public enum CompileStatus
    {
        Idle,
        Compiling,
        CompilationStarted,
        CompilationFailed,
        CompilationComplete
    }

    [Flags]
    public enum EditorScriptCompilationOptions
    {
        BuildingEmpty                               = 0,
        BuildingDevelopmentBuild                    = 1 << 0,
        BuildingForEditor                           = 1 << 1,
        BuildingEditorOnlyAssembly                  = 1 << 2,
        BuildingForIl2Cpp                           = 1 << 3,
        BuildingWithAsserts                         = 1 << 4,
        BuildingIncludingTestAssemblies             = 1 << 5,
        BuildingPredefinedAssembliesAllowUnsafeCode = 1 << 6,
        BuildingForHeadlessPlayer                   = 1 << 7,
        BuildingUseDeterministicCompilation         = 1 << 9,
        BuildingWithRoslynAnalysis                  = 1 << 10,
        BuildingWithoutScriptUpdater                = 1 << 11
    }

    /// <summary>
    /// 封装反射调用的 UnityEditor.Scripting.ScriptCompilation 及相关命名空间内的类型和函数
    /// </summary>
    public static class EditorCompilationWrapper
    {
        public static Type tEditorCompilationInterface { get; private set; }
        public static Type tEditorCompilation { get; private set; }
        public static Type tScriptAssemblySettings { get; private set; }
        public static Type tBeeDriver { get; private set; }

        public static MethodInfo miTickCompilationPipeline { get; private set; }
        public static MethodInfo miBeeDriver_Tick { get; private set; }
        public static MethodInfo miCreateScriptAssemblySettings { get; private set; }
        public static MethodInfo miScriptSettings_SetOutputDirectory { get; private set; }
        public static MethodInfo miCompileScriptsWithSettings { get; private set; }
        public static MethodInfo miRequestScriptCompilation { get; private set; }
        public static MethodInfo miDirtyAllScripts { get; private set; }

        public static object EditorCompilation_Instance { get; private set; }

        static EditorCompilationWrapper()
        {
            tEditorCompilationInterface = typeof(UnityEditor.AssetDatabase).Assembly.GetType("UnityEditor.Scripting.ScriptCompilation.EditorCompilationInterface");
            tEditorCompilation = typeof(UnityEditor.AssetDatabase).Assembly.GetType("UnityEditor.Scripting.ScriptCompilation.EditorCompilation");
            tScriptAssemblySettings = typeof(UnityEditor.AssetDatabase).Assembly.GetType("UnityEditor.Scripting.ScriptCompilation.ScriptAssemblySettings");

            tBeeDriver = (from ass in AppDomain.CurrentDomain.GetAssemblies() where ass.FullName.StartsWith("Bee.BeeDriver") select ass).FirstOrDefault().GetType("Bee.BeeDriver.BeeDriver");
            miBeeDriver_Tick = tBeeDriver.GetMethod("Tick", BindingFlags.Public | BindingFlags.Instance);
            miTickCompilationPipeline = tEditorCompilationInterface.GetMethod("TickCompilationPipeline", BindingFlags.Static | BindingFlags.Public);
            
            foreach (var mi in tEditorCompilation.GetMethods())
            {
                if (mi.Name == "CreateScriptAssemblySettings" && mi.GetParameters().Length == 4)
                    miCreateScriptAssemblySettings = mi;
                else if (mi.Name == "CompileScriptsWithSettings")
                    miCompileScriptsWithSettings = mi;
            }
            miScriptSettings_SetOutputDirectory = tScriptAssemblySettings.GetProperty("OutputDirectory", BindingFlags.Public | BindingFlags.Instance).GetSetMethod();
            miRequestScriptCompilation = tEditorCompilation.GetMethod("RequestScriptCompilation", BindingFlags.Public | BindingFlags.Instance);
            miDirtyAllScripts = tEditorCompilationInterface.GetMethod("DirtyAllScripts", BindingFlags.Public | BindingFlags.Static);

            EditorCompilation_Instance = tEditorCompilationInterface.GetProperty("Instance", BindingFlags.Static | BindingFlags.Public).GetGetMethod().Invoke(null, null);
        }

        public static CompileStatus TickCompilationPipeline(EditorScriptCompilationOptions options, BuildTargetGroup platfromGroup, BuildTarget platform, int subtarget, string[] extraScriptingDefines, bool allowBlocking)
        {
#if UNITY_2022_2_OR_NEWER
            CompileStatus ret = (CompileStatus)miTickCompilationPipeline.Invoke(null, new object[]
            {
                (int)options, platfromGroup, platform, subtarget, extraScriptingDefines, allowBlocking
            });
#else
            CompileStatus ret = (CompileStatus)miTickCompilationPipeline.Invoke(null, new object[]
            {
                (int)options, platfromGroup, platform, subtarget, extraScriptingDefines
            });
#endif
            return ret;
        }

        public static object CreateScriptAssemblySettings(BuildTargetGroup platfromGroup, BuildTarget platform, EditorScriptCompilationOptions options, string[] extraScriptingDefines, string outputDir)
        {
            object ret = miCreateScriptAssemblySettings.Invoke(EditorCompilation_Instance, new object[] { platfromGroup, platform, (int)options, extraScriptingDefines });
            SetScriptAssemblyOutputDir(ret, outputDir);
            return ret;
        }

        public static void SetScriptAssemblyOutputDir(object scriptAssemblySettings, string buildDir)
        {
            miScriptSettings_SetOutputDirectory.Invoke(scriptAssemblySettings, new object[] { buildDir });
        }

        public static CompileStatus CompileScriptsWithSettings(object scriptAssemblySettings)
        {
            CompileStatus ret =  (CompileStatus)miCompileScriptsWithSettings.Invoke(EditorCompilation_Instance, new object[] { scriptAssemblySettings });
            return ret;
        }

        public static void RequestScriptCompilation(string reason)
        {
            //miRequestScriptCompilation.Invoke(EditorCompilation_Instance, new object[] { reason, UnityEditor.Compilation.RequestScriptCompilationOptions.CleanBuildCache });
            miRequestScriptCompilation.Invoke(EditorCompilation_Instance, new object[] { reason, UnityEditor.Compilation.RequestScriptCompilationOptions.None });
        }

        public static void DirtyAllScripts()
        {
            // DirtyAllScripts has been removed since 2021.1
            miDirtyAllScripts?.Invoke(null, new object[] { });
        }
    }

}
#endif