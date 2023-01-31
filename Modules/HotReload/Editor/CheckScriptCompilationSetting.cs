/*
 * Author: Misaka Mikoto
 * email: easy66@live.com
 * github: https://github.com/Misaka-Mikoto-Tech/UnityScriptHotReload
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Callbacks;
using UnityEditor;

namespace ScriptHotReload
{
    /// <summary>
    /// 检查脚本重新编译选项。
    /// 如果开启脚本函数热重载，那么Play时不能自动重新编译脚本
    /// </summary>
    public class CheckScriptCompilationSetting
    {
        const string kScriptCompilationDuringPlay = "ScriptCompilationDuringPlay";

        enum ScriptChangeDuringPlayOptions
        {
            RecompileAndContinuePlaying     = 0,
            RecompileAfterFinishedPlaying   = 1,
            StopPlayingAndRecompile         = 2
        }

        [DidReloadScripts]
        public static void CheckOption()
        {
            var opt = (ScriptChangeDuringPlayOptions)EditorPrefs.GetInt(kScriptCompilationDuringPlay);
            if (opt != ScriptChangeDuringPlayOptions.RecompileAfterFinishedPlaying)
            {
                EditorPrefs.SetInt(kScriptCompilationDuringPlay, (int)ScriptChangeDuringPlayOptions.RecompileAfterFinishedPlaying);
                Debug.LogWarning("[Preferences->General->Script Changes While Playing] has been set to 'RecompileAfterFinishedPlaying'");
            }
        }
    }
}
