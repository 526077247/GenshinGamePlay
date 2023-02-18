using System;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace TaoTie
{
    public class GearConditionExport
    {
        private const string Path = "Assets/Scripts/Code/Module/Gear/ConfigGearConditionGenerate/";
        [MenuItem("Tools/Gear/导出Condition")]
        public static void Export()
        {
            var intnf = typeof(IEventBase);
            Assembly asm = intnf.Assembly;
            foreach (var type in asm.GetTypes())
            {
                if (intnf.IsAssignableFrom(type) && type!=intnf)
                {
                    var fields = type.GetFields();
                    for (int i = 0; i < fields.Length; i++)
                    {
                        if (!fields[i].FieldType.IsClass)
                        {
                            var str = GenerateContent(type, fields[i],out var name);
                            File.WriteAllText(Path+name+".cs",str);
                        }
                    }
                }
            }
        }

        private static string GenerateContent(Type type, FieldInfo fieldInfo,out string className)
        {
            className = $"Config{type.Name}{fieldInfo.Name}Condition";
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("using System;");
            sb.AppendLine("using Nino.Serialization;");
            sb.AppendLine("using Sirenix.OdinInspector;");
            sb.AppendLine("using UnityEngine;");
            sb.AppendLine();
            sb.AppendLine("namespace TaoTie");
            sb.AppendLine("{");
            sb.AppendLine($"    [TriggerType(typeof(Config{type.Name}Trigger))]");
            sb.AppendLine("    [NinoSerialize]");
            sb.AppendLine($"    public partial class {className} : ConfigGearCondition<{type.Name}>");
            sb.AppendLine("    {");
            sb.AppendLine("        [Tooltip(GearTooltips.CompareMode)] [OnValueChanged(\"@CheckModeType(value,mode)\")] ");
            sb.AppendLine("        [NinoMember(1)]");
            sb.AppendLine("        public CompareMode mode;");
            sb.AppendLine("        [NinoMember(2)]");
            if(fieldInfo.GetCustomAttributes(typeof(GearZoneIdAttribute),false).Length!=0)
                sb.AppendLine("        [ValueDropdown(\"@OdinDropdownHelper.GetGearZoneIds()\")]");
            if(fieldInfo.GetCustomAttributes(typeof(GearGroupIdAttribute),false).Length!=0)
                sb.AppendLine("        [ValueDropdown(\"@OdinDropdownHelper.GetGearGroupIds()\")]");
            if(fieldInfo.GetCustomAttributes(typeof(GearActorIdAttribute),false).Length!=0)
                sb.AppendLine("        [ValueDropdown(\"@OdinDropdownHelper.GetGearActorIds()\")]");
            sb.AppendLine($"        public {fieldInfo.FieldType.Name} value;");
            sb.AppendLine();
            sb.AppendLine($"        public override bool IsMatch({type.Name} obj,Gear gear)");
            sb.AppendLine("        {");
            sb.AppendLine($"            return IsMatch(value, obj.{fieldInfo.Name}, mode);");
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