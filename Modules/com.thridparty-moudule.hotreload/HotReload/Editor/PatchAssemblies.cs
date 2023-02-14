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
using Mono;
using Mono.Cecil;
using System.Linq;
using System.Text;

using static ScriptHotReload.HotReloadUtils;
using static ScriptHotReload.HotReloadConfig;

namespace ScriptHotReload
{
    public class GenPatchAssemblies
    {
        public static bool codeHasChanged => methodsToHook.Count > 0;

        public static Dictionary<string, List<MethodData>> methodsToHook { get; private set; } = new Dictionary<string, List<MethodData>>();

        public static int patchNo { get; private set; } = 0;

        private static Dictionary<string, AssemblyData> _assemblyDatas = new Dictionary<string, AssemblyData>();

        [MenuItem("ScriptHotReload/PatchAssemblies")]
        public static void DoGenPatchAssemblies()
        {
            if (!Application.isPlaying)
                return;

            CompileScript.OnCompileSuccess = OnScriptCompileSuccess;
            CompileScript.CompileScriptToDir(kTempCompileToDir);
        }

        public static void OnScriptCompileSuccess(CompileStatus status)
        {
            if (status != CompileStatus.Idle)
                return;

            methodsToHook.Clear();

            var fallbackPathes = GetFallbackAssemblyPaths();
            var baseReadParam = new ReaderParameters(ReadingMode.Deferred)
            { ReadSymbols = true, AssemblyResolver = new HotReloadAssemblyResolver(kBuiltinAssembliesDir, fallbackPathes) };

            var newReadParam = new ReaderParameters(ReadingMode.Deferred)
            { ReadSymbols = true, AssemblyResolver = new HotReloadAssemblyResolver(kTempCompileToDir, fallbackPathes) };

            var writeParam = new WriterParameters() { WriteSymbols = true };

            foreach (string assName in hotReloadAssemblies)
            {
                string assNameNoExt = Path.GetFileNameWithoutExtension(assName);

                string baseDll = $"{kBuiltinAssembliesDir}/{assName}";
                string lastDll = string.Format(kLastDllPathFormat, assNameNoExt);
                string newDll = $"{kTempCompileToDir}/{assName}";

                if (IsFilesEqual(newDll, lastDll))
                    continue;

                using (var baseAssDef = AssemblyDefinition.ReadAssembly(baseDll, baseReadParam))
                {
                    using(var newAssDef = AssemblyDefinition.ReadAssembly(newDll, newReadParam))
                    {
                        var assBuilder = new AssemblyDataBuilder(baseAssDef, newAssDef);
                        if (!assBuilder.DoBuild(patchNo))
                        {
                            Debug.LogError($"[{assName}]不符合热重载条件，停止重载");
                            return;
                        }

                        if (!File.Exists(lastDll))
                            File.Copy(baseDll, lastDll);
                        else
                            File.Copy(newDll, lastDll, true);

                        string patchDll = string.Format(kPatchDllPathFormat, assNameNoExt, patchNo);
                        newAssDef.Write(patchDll, writeParam);

                        // 有可能数量为0，此时虽然与原始dll无差异，但是与上一次编译有差异，也需要处理（清除已hook函数）
                        var methodModified = assBuilder.assemblyData.methodModified;
                        methodsToHook.Add(assName, methodModified.Values.ToList());
                    }
                }
            }
            GC.Collect();
            
            if(methodsToHook.Count == 0)
            {
                Debug.Log("代码没有发生改变，不执行热重载");
                return;
            }

            HookAssemblies.DoHook(methodsToHook);
            if (methodsToHook.Count > 0)
                patchNo++;

            Debug.Log("<color=yellow>热重载完成</color>");
        }
    }

}
