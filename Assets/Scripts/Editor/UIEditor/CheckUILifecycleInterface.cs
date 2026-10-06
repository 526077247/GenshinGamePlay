using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace TaoTie
{
    public static class CheckUILifecycleInterface
    {
        private static readonly string[] ScanDirs =
        {
            "Assets/Scripts/Code/Game/UI",
            "Assets/Scripts/Code/Module/UI/RedDot",
            "Assets/Scripts/Code/Module/UIComponent",
        };

        private static readonly string[] LifecycleMethods = { "OnCreate", "OnEnable", "OnDisable", "OnDestroy" };

        [MenuItem("Tools/校验/检查UI生命周期接口")]
        public static void Check()
        {
            var violations = new List<(string file, string className, List<string> missing)>();
            int scanned = 0;

            foreach (var dir in ScanDirs)
            {
                if (!Directory.Exists(dir)) continue;
                var files = Directory.GetFiles(dir, "*.cs", SearchOption.AllDirectories);
                foreach (var file in files)
                {
                    scanned++;
                    var result = CheckFile(file);
                    if (result != null) violations.Add(result.Value);
                }
            }

            if (violations.Count == 0)
            {
                Debug.Log($"[CheckUILifecycleInterface] 扫描完成，共 {scanned} 个文件，未发现问题。");
                return;
            }

            foreach (var v in violations)
            {
                Debug.LogError($"[CheckUILifecycleInterface] {v.file}\n  类: {v.className}\n  缺少接口: {string.Join(", ", v.missing)}");
            }
            Debug.LogError($"[CheckUILifecycleInterface] 扫描完成，共 {scanned} 个文件，发现 {violations.Count} 个问题。");
        }

        private static (string file, string className, List<string> missing)? CheckFile(string filePath)
        {
            string content = File.ReadAllText(filePath);
            string className = ExtractClassName(content);
            if (className == null) return null;

            var missing = new List<string>();
            foreach (var method in LifecycleMethods)
            {
                if (!HasNonOverrideMethod(content, method)) continue;

                string iface = "IOn" + method.Substring(2);
                if (!HasNonGenericInterface(content, iface))
                {
                    missing.Add(iface);
                }
            }

            if (missing.Count == 0) return null;
            return (MakeRelativePath(filePath), className, missing);
        }

        private static string ExtractClassName(string content)
        {
            var match = Regex.Match(content, @"class\s+(\w+)\s*:");
            return match.Success ? match.Groups[1].Value : null;
        }

        private static bool HasNonOverrideMethod(string content, string method)
        {
            var pattern = @"public\s+(?:virtual\s+)?void\s+" + method + @"\s*\(\s*\)";
            return Regex.IsMatch(content, pattern);
        }

        private static bool HasNonGenericInterface(string content, string iface)
        {
            var pattern = iface + @"(?!\s*<)";
            return Regex.IsMatch(content, pattern);
        }

        private static string MakeRelativePath(string fullPath)
        {
            string projectPath = Application.dataPath.Replace("/Assets", "").Replace("\\", "/");
            string normalized = fullPath.Replace("\\", "/");
            if (normalized.StartsWith(projectPath + "/"))
                return normalized.Substring(projectPath.Length + 1);
            return fullPath;
        }
    }
}
