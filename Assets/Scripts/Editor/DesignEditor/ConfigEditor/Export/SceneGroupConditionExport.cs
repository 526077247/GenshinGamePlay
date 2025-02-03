using System;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;
using Sirenix.OdinInspector;

namespace TaoTie
{
    public class SceneGroupConditionExport
    {
        private const string Path = "Assets/Scripts/Code/Module/Config/SceneGroup/ConfigSceneGroupConditionGenerate/";
        [MenuItem("Tools/导出/导出SceneGroupCondition")]
        public static void Export()
        {
            var intnf = typeof(IEventBase);
            Assembly asm = intnf.Assembly;
            foreach (var type in asm.GetTypes())
            {
                if (intnf.IsAssignableFrom(type) && type!=intnf)
                {
                    if(type.GetCustomAttribute(typeof(SceneGroupGenerateIgnoreAttribute)) is SceneGroupGenerateIgnoreAttribute) continue;
                    var fields = type.GetFields();
                    for (int i = 0; i < fields.Length; i++)
                    {
                        if (!fields[i].FieldType.IsClass)
                        {
                            if(fields[i].GetCustomAttribute(typeof(SceneGroupGenerateIgnoreAttribute)) is SceneGroupGenerateIgnoreAttribute) continue;
                            var str = GenerateContent(type, fields[i],out var name);
                            File.WriteAllText(Path+name+".cs",str);
                        }
                    }
                }
            }
            Debug.Log("导出完成");
        }

        private static string GenerateContent(Type type, FieldInfo fieldInfo,out string className)
        {
            string showText;
            if (fieldInfo.GetCustomAttribute(typeof(LabelTextAttribute)) is LabelTextAttribute labelTextAttribute)
            {
                showText = labelTextAttribute.Text;
            }else{
                showText = ObjectNames.NicifyVariableName(fieldInfo.Name).Replace(" ","");
            }
            className = $"Config{type.Name}{ObjectNames.NicifyVariableName(fieldInfo.Name).Replace(" ","")}Condition";
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("using System;");
            sb.AppendLine("using Nino.Core;");
            sb.AppendLine("using Sirenix.OdinInspector;");
            sb.AppendLine("using UnityEngine;");
            sb.AppendLine();
            sb.AppendLine("namespace TaoTie");
            sb.AppendLine("{");
            sb.AppendLine($"    [TriggerType(typeof(Config{type.Name}Trigger))]");
            sb.AppendLine("    [NinoType(false)]");
            sb.AppendLine($"    [LabelText(\"{showText}\")]");
            sb.AppendLine($"    public partial class {className} : ConfigSceneGroupCondition<{type.Name}>");
            sb.AppendLine("    {");
            sb.AppendLine("        [Tooltip(SceneGroupTooltips.CompareMode)]");
            sb.AppendLine("#if UNITY_EDITOR");
            sb.AppendLine("        [OnValueChanged(\"@\"+nameof(CheckModeType)+\"(\"+nameof(Value)+\",\"+nameof(Mode)+\")\")]");
            sb.AppendLine("#endif");
            sb.AppendLine("        [NinoMember(1)]");
            sb.AppendLine("        [LabelText(\"判断类型\")]");
            sb.AppendLine("        public CompareMode Mode;");
            sb.AppendLine("        [NinoMember(2)]");
            if (fieldInfo.GetCustomAttributes(typeof(SceneGroupZoneIdAttribute), false).Length != 0)
            {
                sb.AppendLine("#if UNITY_EDITOR");
                sb.AppendLine("        [ValueDropdown(\"@\"+nameof(OdinDropdownHelper)+\".\"+nameof(OdinDropdownHelper.GetSceneGroupZoneIds)+\"()\",AppendNextDrawer = true)]");
                sb.AppendLine("        [LabelText(\"区域Id\")]");
                sb.AppendLine("#endif");
            }
            if (fieldInfo.GetCustomAttributes(typeof(SceneGroupSuiteIdAttribute), false).Length != 0)
            {
                sb.AppendLine("#if UNITY_EDITOR");
                sb.AppendLine("        [ValueDropdown(\"@\"+nameof(OdinDropdownHelper)+\".\"+nameof(OdinDropdownHelper.GetSceneGroupSuiteIds)+\"()\",AppendNextDrawer = true)]");
                sb.AppendLine("        [LabelText(\"阶段Id\")]");
                sb.AppendLine("#endif");
            }
            if (fieldInfo.GetCustomAttributes(typeof(SceneGroupActorIdAttribute), false).Length != 0)
            {
                sb.AppendLine("#if UNITY_EDITOR");
                sb.AppendLine("        [ValueDropdown(\"@\"+nameof(OdinDropdownHelper)+\".\"+nameof(OdinDropdownHelper.GetSceneGroupActorIds)+\"()\",AppendNextDrawer = true)]");
                sb.AppendLine("        [LabelText(\"单位Id\")]");
                sb.AppendLine("#endif");
            }
            sb.AppendLine($"        public {fieldInfo.FieldType.Name} Value;");
            sb.AppendLine();
            sb.AppendLine($"        public override bool IsMatch({type.Name} obj, SceneGroup sceneGroup)");
            sb.AppendLine("        {");
            sb.AppendLine($"            return IsMatch(Value, obj.{fieldInfo.Name}, Mode);");
            sb.AppendLine("        }");
            sb.AppendLine("#if UNITY_EDITOR");
            sb.AppendLine("        protected override bool CheckModeType<T>(T t, CompareMode mode)");
            sb.AppendLine("        {");
            sb.AppendLine("            if (!base.CheckModeType(t, mode))");
            sb.AppendLine("            {");
            sb.AppendLine("                mode = CompareMode.Equal;");
            sb.AppendLine("                return false;");
            sb.AppendLine("            }");
            sb.AppendLine();
            sb.AppendLine("            return true;");
            sb.AppendLine("        }");
            sb.AppendLine("#endif");
            sb.AppendLine("    }");
            sb.AppendLine("}");
            return sb.ToString();
        }
    }
}