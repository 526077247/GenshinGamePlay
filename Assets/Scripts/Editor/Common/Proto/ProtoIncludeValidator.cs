using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using ProtoBuf;
using UnityEditor;
using UnityEngine;

namespace TaoTie
{
    /// <summary>
    /// 检测并自动补齐未添加 [ProtoInclude] 多态标签的 [ProtoContract] 类型。
    /// 菜单: Tools/Proto/Validate ProtoInclude  — 仅检测报告
    /// 菜单: Tools/Proto/Auto Fix ProtoInclude  — 检测并自动写入源文件
    /// 菜单: Tools/Proto/Re-export All .bytes   — 从 .json 重新导出所有 ProtoBuf .bytes
    /// </summary>
    public static class ProtoIncludeValidator
    {
        private const int StartTag = 100;

        // JSON 文件名模式 → 反序列化的目标类型
        // 顺序很重要：更具体的模式放在前面
        private static readonly (string pattern, Type type)[] JsonToTypeMap =
        {
            ("ConfigActor.json", typeof(ConfigActor)),
            ("ConfigAIBeta", typeof(ConfigAIBeta)),
            ("Abilities", typeof(ConfigAbility[])),
            ("AITree", typeof(ConfigAIDecisionTree)),
            ("FsmConfig", typeof(ConfigFsmController)),
            ("PoseConfig", typeof(ConfigFsmController)),
            ("ConfigFsmController", typeof(ConfigFsmController)),
            ("SceneGroup", typeof(ConfigSceneGroup)),
            ("/Story/", typeof(ConfigStory)),
            ("ConfigCameras", typeof(ConfigCameras)),
            ("ConfigEnvironments", typeof(ConfigEnvironments)),
            ("ConfigInput", typeof(ConfigInput)),
        };

        [MenuItem("Tools/Proto/Validate ProtoInclude")]
        public static void Validate()
        {
            var missing = FindMissingProtoIncludes();
            if (missing.Count == 0)
            {
                Debug.Log("[ProtoIncludeValidator] ✅ All ProtoContract types have correct ProtoInclude attributes.");
                return;
            }

            var sb = new StringBuilder();
            sb.AppendLine($"[ProtoIncludeValidator] ❌ Found {missing.Count} base type(s) with missing ProtoInclude:");
            foreach (var entry in missing)
            {
                sb.AppendLine($"  {entry.Key.FullName} ({entry.Value.Count} missing):");
                foreach (var sub in entry.Value)
                    sb.AppendLine($"    → {sub.FullName}");
            }
            Debug.LogWarning(sb.ToString());
        }

        [MenuItem("Tools/Proto/Re-export All .bytes")]
        public static void ReExportAllBytes()
        {
            // 搜索 Assets/AssetsPackage 下所有 .json 文件
            var jsonGuids = AssetDatabase.FindAssets("t:TextAsset", new[] { "Assets/AssetsPackage" });
            int exported = 0, skipped = 0, failed = 0;
            var sb = new StringBuilder();

            foreach (var guid in jsonGuids)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                if (!assetPath.EndsWith(".json")) continue;

                // 通过文件名匹配目标类型
                Type targetType = null;
                foreach (var kv in JsonToTypeMap)
                {
                    if (assetPath.Contains(kv.pattern))
                    {
                        targetType = kv.type;
                        break;
                    }
                }
                if (targetType == null)
                {
                    skipped++;
                    continue;
                }

                // 检查是否存在对应的 .bytes 文件
                var bytesPath = assetPath.Replace(".json", ".bytes");
                if (!File.Exists(Path.GetFullPath(bytesPath)))
                {
                    skipped++;
                    continue;
                }

                try
                {
                    var jsonText = File.ReadAllText(Path.GetFullPath(assetPath));
                    var obj = JsonHelper.FromJson(targetType, jsonText);
                    if (obj == null)
                    {
                        failed++;
                        sb.AppendLine($"  ❌ {assetPath}: JSON deserialized to null");
                        continue;
                    }

                    var bytes = ProtobufHelper.ToBytes(obj);
                    File.WriteAllBytes(Path.GetFullPath(bytesPath), bytes);
                    exported++;
                }
                catch (Exception ex)
                {
                    failed++;
                    sb.AppendLine($"  ❌ {assetPath}: {ex.Message.Split('\n', '\r')[0]}");
                }
            }

            AssetDatabase.Refresh();
            // 强制重新导入所有 .bytes 以刷新 Unity 缓存
            var bytesGuids = AssetDatabase.FindAssets("t:TextAsset", new[] { "Assets/AssetsPackage" });
            foreach (var guid in bytesGuids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.EndsWith(".bytes"))
                    AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            }

            sb.Insert(0, $"[ProtoIncludeValidator] Re-export complete: {exported} exported, {skipped} skipped, {failed} failed.\n");
            if (failed > 0)
                Debug.LogWarning(sb.ToString());
            else
                Debug.Log(sb.ToString());
        }

        [MenuItem("Tools/Proto/Auto Fix ProtoInclude")]
        public static void AutoFix()
        {
            var missing = FindMissingProtoIncludes();
            if (missing.Count == 0)
            {
                Debug.Log("[ProtoIncludeValidator] ✅ Nothing to fix. All ProtoInclude attributes are present.");
                return;
            }

            int fixedTypes = 0;
            int fixedEntries = 0;
            foreach (var entry in missing)
            {
                var baseType = entry.Key;
                var derivedTypes = entry.Value;
                var filePath = FindSourceFile(baseType);
                if (filePath == null)
                {
                    Debug.LogError($"[ProtoIncludeValidator] Cannot find source file for {baseType.FullName}, skipping.");
                    continue;
                }

                var content = File.ReadAllText(filePath);
                var lines = content.Split('\n');
                int insertLine = -1;
                // Find the [ProtoContract] line for this specific class
                string classDecl = FindClassDeclarationLine(baseType);
                if (string.IsNullOrEmpty(classDecl))
                {
                    Debug.LogError($"[ProtoIncludeValidator] Cannot find class declaration for {baseType.FullName} in {filePath}, skipping.");
                    continue;
                }

                // Find the line index of the class declaration
                for (int i = 0; i < lines.Length; i++)
                {
                    if (lines[i].Contains(classDecl))
                    {
                        insertLine = i;
                        break;
                    }
                }

                if (insertLine < 0)
                {
                    Debug.LogError($"[ProtoIncludeValidator] Cannot find class line '{classDecl}' in {filePath}, skipping.");
                    continue;
                }

                // Find existing max ProtoMember tag on this base type to avoid conflicts
                int nextTag = GetMaxProtoMemberTag(baseType) + 1;
                if (nextTag < StartTag) nextTag = StartTag;

                // Also check existing ProtoInclude tags from compiled assembly
                var existingIncludes = baseType.GetCustomAttributes<ProtoIncludeAttribute>();
                foreach (var inc in existingIncludes)
                {
                    if (inc.Tag >= nextTag) nextTag = inc.Tag + 1;
                }

                // Also check existing ProtoInclude tags from source file content
                // (handles multiple derived types processed in the same run)
                var contentForTags = File.ReadAllText(filePath);
                var includeTagMatches = System.Text.RegularExpressions.Regex.Matches(contentForTags,
                    @"ProtoInclude\((\d+),");
                foreach (System.Text.RegularExpressions.Match m in includeTagMatches)
                {
                    int tag = int.Parse(m.Groups[1].Value);
                    if (tag >= nextTag) nextTag = tag + 1;
                }

                var newLines = new List<string>();
                for (int i = 0; i < lines.Length; i++)
                {
                    newLines.Add(lines[i]);
                    if (i == insertLine)
                    {
                        // Insert ProtoInclude attributes right after the [ProtoContract] line
                        // Actually insert them BEFORE the class declaration line (after [ProtoContract])
                        // We need to find the line with [ProtoContract] that's closest before the class declaration
                    }
                }

                // Better approach: find the [ProtoContract] attribute line that precedes the class declaration
                // and insert ProtoInclude after it
                int protoContractLine = -1;
                for (int i = insertLine; i >= 0; i--)
                {
                    if (lines[i].Contains("[ProtoContract]") || lines[i].Contains("[ProtoContract("))
                    {
                        protoContractLine = i;
                        break;
                    }
                }

                if (protoContractLine < 0)
                {
                    // [ProtoContract] might be on the same line as class declaration or in a different format
                    // Insert before the class declaration line
                    protoContractLine = insertLine - 1;
                    if (protoContractLine < 0) protoContractLine = 0;
                }

                // Build the new content with ProtoInclude attributes inserted
                var sb = new StringBuilder();
                for (int i = 0; i < lines.Length; i++)
                {
                    sb.Append(lines[i]);
                    if (i == protoContractLine)
                    {
                        // Insert ProtoInclude attributes right after the [ProtoContract] line
                        foreach (var derived in derivedTypes)
                        {
                            sb.Append($"\n    [ProtoInclude({nextTag}, typeof({GetSimpleTypeName(derived)}))]");
                            nextTag++;
                            fixedEntries++;
                        }
                    }
                    if (i < lines.Length - 1)
                        sb.Append("\n");
                }

                File.WriteAllText(filePath, sb.ToString());
                fixedTypes++;
            }

            AssetDatabase.Refresh();
            Debug.Log($"[ProtoIncludeValidator] ✅ Fixed {fixedTypes} base type(s), added {fixedEntries} ProtoInclude attribute(s).");
        }

        /// <summary>
        /// Scan all assemblies for [ProtoContract] types, build inheritance trees,
        /// and find base classes that are missing [ProtoInclude] for their derived types.
        /// </summary>
        private static Dictionary<Type, List<Type>> FindMissingProtoIncludes()
        {
            var result = new Dictionary<Type, List<Type>>();

            // Collect all ProtoContract types
            var allTypes = new List<Type>();
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type[] types;
                try { types = asm.GetTypes(); }
                catch { continue; }
                allTypes.AddRange(types);
            }

            var protoTypes = allTypes
                .Where(t => t.GetCustomAttribute<ProtoContractAttribute>() != null)
                .ToList();

            // Build: baseType -> list of DIRECT derived ProtoContract types only.
            // protobuf-net requires [ProtoInclude] on the DIRECT parent only;
            // it walks the chain automatically at runtime.
            // For generic type definition subclasses (e.g. ConfigSceneGroupTrigger<T>),
            // we also enumerate all constructed versions so the root can declare them.
            var hierarchy = new Dictionary<Type, List<Type>>();
            foreach (var t in protoTypes)
            {
                var baseType = t.BaseType;
                if (baseType == null || baseType == typeof(object)) continue;
                if (!hierarchy.TryGetValue(baseType, out var list))
                {
                    list = new List<Type>();
                    hierarchy[baseType] = list;
                }
                if (!list.Contains(t))
                    list.Add(t);
            }

            // For each base type that also has [ProtoContract], check its [ProtoInclude] attributes
            foreach (var entry in hierarchy)
            {
                var baseType = entry.Key;
                var derivedTypes = entry.Value;

                // Only check base types that also have [ProtoContract]
                if (baseType.GetCustomAttribute<ProtoContractAttribute>() == null) continue;

                // Skip open generic types as base - can't add attributes to them
                if (baseType.IsGenericTypeDefinition) continue;

                var existingIncludes = baseType.GetCustomAttributes<ProtoIncludeAttribute>()
                    .Select(a => a.KnownType)
                    .ToHashSet();

                // For constructed generic base types, also include ProtoInclude attributes
                // from the generic type definition (they apply to all constructions)
                if (baseType.IsGenericType && !baseType.IsGenericTypeDefinition)
                {
                    var genericDef = baseType.GetGenericTypeDefinition();
                    foreach (var inc in genericDef.GetCustomAttributes<ProtoIncludeAttribute>())
                    {
                        existingIncludes.Add(inc.KnownType);
                    }
                }

                // Only check DIRECT subclasses (not full chain).
                // For generic type definition subclasses (e.g. ConfigSceneGroupTrigger<T>),
                // find all constructed versions and include those instead.
                var missing = new List<Type>();
                foreach (var d in derivedTypes)
                {
                    if (d.IsGenericTypeDefinition)
                    {
                        // Find all constructed versions of this generic type in the ProtoContract set
                        foreach (var pt in protoTypes)
                        {
                            if (pt.IsGenericType && pt.GetGenericTypeDefinition() == d && !existingIncludes.Contains(pt))
                            {
                                if (!missing.Contains(pt))
                                    missing.Add(pt);
                            }
                        }
                        // Don't add the open generic definition itself — can't use typeof(SomeGeneric<>)
                        continue;
                    }
                    else if (!existingIncludes.Contains(d))
                    {
                        // For constructed generic base types (e.g. ConfigSceneGroupCondition<EnterZoneEvent>),
                        // also check ProtoInclude attributes from the generic type definition
                        if (baseType.IsGenericType && !baseType.IsGenericTypeDefinition)
                        {
                            var genericDef = baseType.GetGenericTypeDefinition();
                            var defIncludes = genericDef.GetCustomAttributes<ProtoIncludeAttribute>()
                                .Select(a => a.KnownType)
                                .ToHashSet();
                            if (!defIncludes.Contains(d))
                                missing.Add(d);
                        }
                        else
                        {
                            missing.Add(d);
                        }
                    }
                }

                // Filter out false positives:
                // 1. Open generic type definitions (can't add [ProtoInclude(typeof(SomeGeneric<>))])
                //    Their constructed versions are checked separately above.
                // 2. For constructed generic base types, leaf types declared on the generic definition
                //    are already inherited — no action needed.
                missing = missing
                    .Where(d => !d.IsGenericTypeDefinition)
                    .Where(d =>
                    {
                        // For constructed generic base types, check generic definition's ProtoInclude too
                        if (baseType.IsGenericType && !baseType.IsGenericTypeDefinition)
                        {
                            var genericDef = baseType.GetGenericTypeDefinition();
                            var defIncludes = genericDef.GetCustomAttributes<ProtoIncludeAttribute>()
                                .Select(a => a.KnownType);
                            foreach (var inc in defIncludes)
                            {
                                if (inc != null && inc.FullName == d.FullName)
                                    return false;
                            }
                        }
                        return true;
                    })
                    .ToList();

                if (missing.Count > 0)
                {
                    result[baseType] = missing;
                }
            }

            return result;
        }

        /// <summary>
        /// Find the .cs source file for a given type using AssetDatabase.
        /// </summary>
        private static string FindSourceFile(Type type)
        {
            // For constructed generic types (e.g. ConfigParam<bool>), use the generic definition
            if (type.IsGenericType && !type.IsGenericTypeDefinition)
                type = type.GetGenericTypeDefinition();

            var typeName = type.Name;
            // Strip generic arity suffix (e.g. ConfigParam`1 -> ConfigParam)
            var tickIndex = typeName.IndexOf('`');
            if (tickIndex > 0)
                typeName = typeName.Substring(0, tickIndex);
            bool isGeneric = type.IsGenericType;

            var guids = AssetDatabase.FindAssets($"{typeName} t:Script");
            // First pass: exact filename match
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.EndsWith($"{typeName}.cs"))
                {
                    return Path.GetFullPath(path);
                }
            }
            // Second pass: search file content with word boundary
            // For generic types, look for "class TypeName<" to avoid matching "class TypeNameBool"
            // For non-generic, look for "class TypeName " or "class TypeName:" or "class TypeName\n"
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.EndsWith(".cs"))
                {
                    var content = File.ReadAllText(path);
                    // Use regex with word boundary to avoid partial matches
                    var pattern = isGeneric
                        ? $@"\bclass\s+{typeName}\s*<"
                        : $@"\bclass\s+{typeName}\b";
                    if (System.Text.RegularExpressions.Regex.IsMatch(content, pattern))
                    {
                        return Path.GetFullPath(path);
                    }
                }
            }
            // Third pass: broad search across ALL script files (handles cases where
            // a class is declared in a file with a different name, e.g. ConfigConditionByData<T> in ConfigCondition.cs)
            var allScriptGuids = AssetDatabase.FindAssets("t:Script");
            foreach (var guid in allScriptGuids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.EndsWith(".cs"))
                {
                    var content = File.ReadAllText(path);
                    var pattern = isGeneric
                        ? $@"\bclass\s+{typeName}\s*<"
                        : $@"\bclass\s+{typeName}\b";
                    if (System.Text.RegularExpressions.Regex.IsMatch(content, pattern))
                    {
                        return Path.GetFullPath(path);
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Find the class declaration line for a type (e.g., "public abstract class ConfigAbilityAction")
        /// </summary>
        private static string FindClassDeclarationLine(Type type)
        {
            // For constructed generic types, use the generic definition
            if (type.IsGenericType && !type.IsGenericTypeDefinition)
                type = type.GetGenericTypeDefinition();

            var name = type.Name;
            // For generic types, the declaration uses the generic parameter
            if (type.IsGenericType)
            {
                // e.g., ConfigParam<T> -> look for "class ConfigParam<"
                var tickIndex = name.IndexOf('`');
                if (tickIndex > 0)
                    name = name.Substring(0, tickIndex);
                return $"class {name}<";
            }
            return $"class {name}";
        }

        /// <summary>
        /// Get the maximum [ProtoMember] tag used on a type.
        /// </summary>
        private static int GetMaxProtoMemberTag(Type type)
        {
            int maxTag = 0;
            foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                var attr = field.GetCustomAttribute<ProtoMemberAttribute>();
                if (attr != null && attr.Tag > maxTag)
                    maxTag = attr.Tag;
            }
            foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                var attr = prop.GetCustomAttribute<ProtoMemberAttribute>();
                if (attr != null && attr.Tag > maxTag)
                    maxTag = attr.Tag;
            }
            return maxTag;
        }

        /// <summary>
        /// Get a simple type name for use in typeof() — handles nested types and generics.
        /// </summary>
        private static string GetSimpleTypeName(Type type)
        {
            if (type.IsGenericType)
            {
                // e.g., ConfigParam<bool> -> ConfigParam<bool>
                var name = type.Name;
                var tickIndex = name.IndexOf('`');
                if (tickIndex > 0)
                    name = name.Substring(0, tickIndex);
                var args = string.Join(", ", type.GetGenericArguments().Select(GetSimpleTypeName));
                return $"{name}<{args}>";
            }
            if (type.IsNested)
            {
                return $"{GetSimpleTypeName(type.DeclaringType)}.{type.Name}";
            }
            return type.Name;
        }
    }
}
