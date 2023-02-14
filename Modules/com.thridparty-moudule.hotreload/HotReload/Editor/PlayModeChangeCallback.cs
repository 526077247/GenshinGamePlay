/*
 * Author: Misaka Mikoto
 * email: easy66@live.com
 * github: https://github.com/Misaka-Mikoto-Tech/UnityScriptHotReload
 */
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Security.Cryptography;
using System;
using System.Reflection;
using Mono.Cecil;
using UnityEditor;

using static ScriptHotReload.HotReloadConfig;

namespace ScriptHotReload
{
    [InitializeOnLoad]
    public static class PlayModeChangeCallback
    {
        static PlayModeChangeCallback()
        {
            EditorApplication.playModeStateChanged += OnPlayModeChange;
        }

        static void OnPlayModeChange(PlayModeStateChange mode)
        {
            switch (mode)
            {
                case PlayModeStateChange.EnteredPlayMode:
                    {
                        HotReloadUtils.RemoveAllFiles(kTempScriptDir);
                        if (Directory.Exists(kTempCompileToDir))
                            Directory.Delete(kTempCompileToDir);

                        CompileScript.ResetCompileStatus();
                        string json = EditorPrefs.GetString(kEditorScriptBuildParamsKey);
                        if (!string.IsNullOrEmpty(json))
                            CompileScript.editorBuildParams = JsonUtility.FromJson<CompileScript.EditorBuildParams>(json);
                        break;
                    }
                case PlayModeStateChange.ExitingEditMode: // 退出编辑模式保存编译参数
                    {
                        string json = JsonUtility.ToJson(CompileScript.editorBuildParams);
                        EditorPrefs.SetString(kEditorScriptBuildParamsKey, json);
                        break;
                    }
                case PlayModeStateChange.ExitingPlayMode:
                    {
                        CompileScript.ResetCompileStatus();
                        if (GenPatchAssemblies.codeHasChanged)
                        {
                            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
                            EditorCompilationWrapper.RequestScriptCompilation("运行过程中代码被修改");
                        }
                        break;
                    }
                case PlayModeStateChange.EnteredEditMode:
                    {
                        // HotReloadUtils.RemoveAllFiles(kTempScriptDir); // TODO 改成调用 cmd 延迟删除，否则会提示占用
                        break;
                    }
            }
        }
    }
}

