/*
 * Author: Misaka Mikoto
 * email: easy66@live.com
 * github: https://github.com/Misaka-Mikoto-Tech/UnityScriptHotReload
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using static UnityEngine.GraphicsBuffer;
using UnityEditor.Build.Player;
using System.IO;
using UnityEditor.Callbacks;
using System.Reflection;
using MonoHook;
using System.Runtime.CompilerServices;
using System;
using System.Reflection.Emit;

using static ScriptHotReload.HotReloadUtils;

namespace ScriptHotReload
{
    /// <summary>
    /// 手动编译Editor下的脚本到指定目录
    /// </summary>
    public static class CompileScript
    {
        public static CompileStatus compileStatus { get; private set; }

        public static Action<CompileStatus> OnCompileSuccess;
        public static EditorBuildParams editorBuildParams;

        [Serializable]
        public struct EditorBuildParams
        {
            public EditorScriptCompilationOptions   options;
            public BuildTargetGroup                 platformGroup;
            public BuildTarget                      platform;
            public int                              subtarget;
            public string[]                         extraScriptingDefines;
            public bool                             allowBlocking;

            public string                           outputDir;
        }
        
        static bool s_CompileRequested = false;

        public static void CompileScriptToDir(string outputDir)
        {
            if(!IsIdle())
            {
                Debug.LogError($"当前编译状态:{compileStatus}, 不允许执行编译");
                return;
            }

            // 生成编译配置并指定输出目录
            editorBuildParams.outputDir = outputDir;
            object scriptAssemblySettings = EditorCompilationWrapper.CreateScriptAssemblySettings(
                editorBuildParams.platformGroup, editorBuildParams.platform, editorBuildParams.options, editorBuildParams.extraScriptingDefines, outputDir);
            
            Directory.CreateDirectory(outputDir);
            RemoveAllFiles(outputDir);
            EditorCompilationWrapper.DirtyAllScripts();
            var status = EditorCompilationWrapper.CompileScriptsWithSettings(scriptAssemblySettings);
            Debug.Log($"开始编译dll到目录: {outputDir}");
            s_CompileRequested = true;

            ManualTickCompilationPipeline();
        }


        [DidReloadScripts]
        static void Init()
        {
            {// install hook
                var miOri = EditorCompilationWrapper.miTickCompilationPipeline;
                var miNew = typeof(CompileScript).GetMethod(nameof(TickCompilationPipeline), BindingFlags.NonPublic | BindingFlags.Static);
                var miReplace = typeof(CompileScript).GetMethod(nameof(TickCompilationPipeline_Proxy), BindingFlags.NonPublic | BindingFlags.Static);
                new MethodHook(miOri, miNew, miReplace).Install();
            }

            EditorApplication.update += EditorApplication_Update;
        }

        static void EditorApplication_Update()
        {
            if(s_CompileRequested)
            {
                if(IsIdle())
                {
                    s_CompileRequested = false;
                    OnCompileSuccess?.Invoke(compileStatus);

                    if (compileStatus == CompileStatus.Idle)
                    {
                        Debug.Log("编译已完成");
                    }
                    else
                    {
                        Debug.LogError($"编译失败:{compileStatus}");
                    }
                }
                else if(Application.isPlaying) // PlayMode 下Unity会停止调用 TickCompilationPipeline, 导致编译请求进度无法前进，所以需要我们手动去执行
                {
                    ManualTickCompilationPipeline();
                }
            }
        }

        static bool IsIdle()
        {
            return compileStatus == CompileStatus.Idle || compileStatus == CompileStatus.CompilationFailed;
        }

        public static void ResetCompileStatus()
        {
            s_CompileRequested = false;
            compileStatus = CompileStatus.Idle;
        }

        static void ManualTickCompilationPipeline()
        {
            compileStatus = EditorCompilationWrapper.TickCompilationPipeline(
                        editorBuildParams.options, editorBuildParams.platformGroup, editorBuildParams.platform,
                        editorBuildParams.subtarget, editorBuildParams.extraScriptingDefines, editorBuildParams.allowBlocking);
        }


#if UNITY_2022_2_OR_NEWER
        /// <summary>
        /// 拦截Unity自己的Editor编译函数获取编译参数
        /// </summary>
        /// <param name="options">type:EditorScriptCompilationOptions</param>
        /// <remarks>此函数每帧都会被调用，即使当前无需编译</remarks>
        static CompileStatus TickCompilationPipeline(EditorScriptCompilationOptions options, BuildTargetGroup platfromGroup, BuildTarget platform, int subtarget, string[] extraScriptingDefines, bool allowBlocking)
        {
            editorBuildParams.options = options;
            editorBuildParams.platformGroup = platfromGroup;
            editorBuildParams.platform = platform;
            editorBuildParams.subtarget = subtarget;
            editorBuildParams.extraScriptingDefines = extraScriptingDefines;
            editorBuildParams.allowBlocking = allowBlocking;

            compileStatus = TickCompilationPipeline_Proxy(options, platfromGroup, platform, subtarget, extraScriptingDefines, allowBlocking);
            //Debug.Log($"TickCompilationPipleline with status:{s_CompileStatus}");
            return compileStatus;
        }

        [MethodImpl(MethodImplOptions.NoOptimization)]
        static CompileStatus TickCompilationPipeline_Proxy(EditorScriptCompilationOptions options, BuildTargetGroup platfromGroup, BuildTarget platform, int subtarget, string[] extraScriptingDefines, bool allowBlocking)
        {
            Debug.Log($"dummy code " + platfromGroup.GetType());
            return CompileStatus.Idle;
        }
#elif UNITY_2021_2_OR_NEWER
        /// <summary>
        /// 拦截Unity自己的Editor编译函数获取编译参数
        /// </summary>
        /// <param name="options">type:EditorScriptCompilationOptions</param>
        /// <remarks>此函数每帧都会被调用，即使当前无需编译</remarks>
        static CompileStatus TickCompilationPipeline(EditorScriptCompilationOptions options, BuildTargetGroup platfromGroup, BuildTarget platform, int subtarget, string[] extraScriptingDefines)
        {
            editorBuildParams.options = options;
            editorBuildParams.platformGroup = platfromGroup;
            editorBuildParams.platform = platform;
            editorBuildParams.subtarget = subtarget;
            editorBuildParams.extraScriptingDefines = extraScriptingDefines;

            compileStatus = TickCompilationPipeline_Proxy(options, platfromGroup, platform, subtarget, extraScriptingDefines);
            //Debug.Log($"TickCompilationPipleline with status:{s_CompileStatus}");
            return compileStatus;
        }

        [MethodImpl(MethodImplOptions.NoOptimization)]
        static CompileStatus TickCompilationPipeline_Proxy(EditorScriptCompilationOptions options, BuildTargetGroup platfromGroup, BuildTarget platform, int subtarget, string[] extraScriptingDefines)
        {
            Debug.Log($"dummy code " + platfromGroup.GetType());
            return CompileStatus.Idle;
        }
#elif UNITY_2020_1_OR_NEWER
        /// <summary>
        /// 拦截Unity自己的Editor编译函数获取编译参数
        /// </summary>
        /// <param name="options">type:EditorScriptCompilationOptions</param>
        /// <remarks>此函数每帧都会被调用，即使当前无需编译</remarks>
        static CompileStatus TickCompilationPipeline(EditorScriptCompilationOptions options, BuildTargetGroup platfromGroup, BuildTarget platform, string[] extraScriptingDefines)
        {
            editorBuildParams.options = options;
            editorBuildParams.platformGroup = platfromGroup;
            editorBuildParams.platform = platform;
            editorBuildParams.extraScriptingDefines = extraScriptingDefines;

            compileStatus = TickCompilationPipeline_Proxy(options, platfromGroup, platform, extraScriptingDefines);
            //Debug.Log($"TickCompilationPipleline with status:{s_CompileStatus}");
            return compileStatus;
        }

        [MethodImpl(MethodImplOptions.NoOptimization)]
        static CompileStatus TickCompilationPipeline_Proxy(EditorScriptCompilationOptions options, BuildTargetGroup platfromGroup, BuildTarget platform, string[] extraScriptingDefines)
        {
            Debug.Log($"dummy code " + platfromGroup.GetType());
            return CompileStatus.Idle;
        }
#else
        /// <summary>
        /// 拦截Unity自己的Editor编译函数获取编译参数
        /// </summary>
        /// <param name="options">type:EditorScriptCompilationOptions</param>
        /// <remarks>此函数每帧都会被调用，即使当前无需编译</remarks>
        static CompileStatus TickCompilationPipeline(EditorScriptCompilationOptions options, BuildTargetGroup platfromGroup, BuildTarget platform)
        {
            editorBuildParams.options = options;
            editorBuildParams.platformGroup = platfromGroup;
            editorBuildParams.platform = platform;

            compileStatus = TickCompilationPipeline_Proxy(options, platfromGroup, platform);
            //Debug.Log($"TickCompilationPipleline with status:{s_CompileStatus}");
            return compileStatus;
        }

        [MethodImpl(MethodImplOptions.NoOptimization)]
        static CompileStatus TickCompilationPipeline_Proxy(EditorScriptCompilationOptions options, BuildTargetGroup platfromGroup, BuildTarget platform)
        {
            Debug.Log($"dummy code " + platfromGroup.GetType());
            return CompileStatus.Idle;
        }
#endif
    }
}
