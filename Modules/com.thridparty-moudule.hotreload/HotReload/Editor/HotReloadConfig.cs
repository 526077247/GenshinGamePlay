/*
 * Author: Misaka Mikoto
 * email: easy66@live.com
 * github: https://github.com/Misaka-Mikoto-Tech/UnityScriptHotReload
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.CompilerServices;
using System.Security.Permissions;
using SecurityAction = System.Security.Permissions.SecurityAction;

/*
 * 安全相关特性，如果生成的patch dll出现 xx 字段/方法 `FieldAccessException` 异常，可以根据当前运行时开放下列其中之一
 * 
 * #pragma warning disable 618
 * [assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)] // for mono and .net framework < 4.0
 * [assembly: IgnoresAccessChecksTo("TestDll")] // for .net core, and pls modify assembly name
 * #pragma warning restore 618
 */

namespace ScriptHotReload
{
    public class HotReloadConfig
    {
        /// <summary>
        /// 需要执行 HotReload 的程序集名称
        /// </summary>
        public static List<string> hotReloadAssemblies = new List<string>()
        {
            "Unity.Code.dll"
        };

        public const string kPatchAssemblyName      = "{0}_patch_{1}";  // {0}:assNameNoExt, {1}:PatchNo

        public const string kTempScriptDir          = "Temp/ScriptHotReload";
        public const string kTempCompileToDir       = "Temp/ScriptHotReload/tmp";
        public const string kBuiltinAssembliesDir   = "Library/ScriptAssemblies";
        public const string kLastDllPathFormat      = kTempScriptDir + "/{0}__last.dll"; // {0}:assNameNoExt
        public const string kPatchDllPathFormat     = kTempScriptDir + "/"+ kPatchAssemblyName +".dll";

        
        public const string kEditorScriptBuildParamsKey = "kEditorScriptBuildParamsKey";
        /// <summary>
        /// 类型内 lambda 表达式的封装类名称，代码修改后lambda表达式对应的函数名可能会发生改变，因此特殊处理
        /// </summary>
        /// <remarks>注意：只修改lambda表达式，但不修改调用处代码，将不会被检测到改变</remarks>
        public const string kLambdaWrapperBackend = "/<>c";

    }

}
