#define NOT_SERVER //导服务端配置开关
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using OfficeOpenXml;
using ProtoBuf;
using TaoTie.LitJson;
using LicenseContext = OfficeOpenXml.LicenseContext;

namespace TaoTie
{
    public enum ConfigType
    {
        c = 0,
        s = 1,
    }

    class HeadInfo
    {
        public string FieldAttribute;
        public string FieldDesc;
        public string FieldName;
        public string FieldType;
        public int FieldIndex;

        public HeadInfo(string cs, string desc, string name, string type, int index)
        {
            this.FieldAttribute = cs;
            this.FieldDesc = desc;
            this.FieldName = name;
            this.FieldType = type;
            this.FieldIndex = index;
        }
    }

    // 这里加个标签是为了防止编译时裁剪掉protobuf，因为整个tool工程没有用到protobuf，编译会去掉引用，然后动态编译就会出错
    [ProtoContract()]
    class Table
    {
        [ProtoMember(1)]
        public bool C;
#if !NOT_SERVER
        public bool S;
#endif
        public int Index;
        public Dictionary<string, HeadInfo> HeadInfos = new Dictionary<string, HeadInfo>();
    }
    public partial class ExcelExporter
    {
        private static string template;

        private const string clientClassDir = "../Assets/Scripts/Code/Module/Generate/Config";
        private static string ClientClassDir
        {
            get
            {
                if (IsCheck) return "./Temp/ClientClass";
                return clientClassDir;
            }
        }
#if !NOT_SERVER
        private const string serverClassDir = "../Server/Model/Generate/Config";
        private static string ServerClassDir
        {
            get
            {
                if (IsCheck) return "./Temp/ServerClass";
                return serverClassDir;
            }
        }
#endif
        private const string excelDir = "../Excel";

        private const string jsonDir = "../Excel/Json/{0}/{1}";

        private const string __clientProtoDir = "../Assets/AssetsPackage/Config/{0}";
        private static string clientProtoDir
        {
            get
            {
                if (IsCheck) return "./Temp/ClientProto/{0}";
                return __clientProtoDir;
            }
        }
#if !NOT_SERVER
        private const string __serverProtoDir = "../Config/{0}";
        private static string serverProtoDir
        {
            get
            {
                if (IsCheck) return "./Temp/ServerProto/{0}";
                return __serverProtoDir;
            }
        }
#endif
        private static bool IsCheck;

        private static Assembly[] configAssemblies = new Assembly[2];

        private static Dictionary<string, Table> tables = new Dictionary<string, Table>();
        private static Dictionary<string, ExcelPackage> packages = new Dictionary<string, ExcelPackage>();
        private static HashSet<string> neededArrayWrappers = new HashSet<string>();

        private static Table GetTable(string protoName)
        {
            if (!tables.TryGetValue(protoName, out var table))
            {
                table = new Table();
                tables[protoName] = table;
            }

            return table;
        }

        public static ExcelPackage GetPackage(string filePath)
        {
            if (!packages.TryGetValue(filePath, out var package))
            {
                using Stream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                package = new ExcelPackage(stream);
                packages[filePath] = package;
            }

            return package;
        }
       
        public static void Export(bool isCheck = false)
        {
            IsCheck = isCheck;
            if (isCheck)
                Console.WriteLine("ExcelExporter 校验");
            else
                Console.WriteLine("ExcelExporter 开始");
            try
            {
                neededArrayWrappers.Clear();
                template = File.ReadAllText("Template.txt");
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                if (Directory.Exists(ClientClassDir))
                {
                    Directory.Delete(ClientClassDir, true);
                }
#if !NOT_SERVER
                if (Directory.Exists(ServerClassDir))
                {
                    Directory.Delete(ServerClassDir, true);
                }
#endif
                if (Directory.Exists(clientProtoDir))
                {
                    Directory.Delete(clientProtoDir, true);
                }

                foreach (string path in ExportHelper.FindFile(excelDir))
                {
                    string fileName = Path.GetFileName(path);
                    if (!fileName.EndsWith(".xlsx") || fileName.StartsWith("~$") || fileName.Contains("#"))
                    {
                        continue;
                    }

                    string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
                    string fileNameWithoutCS = fileNameWithoutExtension;
                    string cs = "cs";
                    if (fileNameWithoutExtension.Contains("@"))
                    {
                        string[] ss = fileNameWithoutExtension.Split("@");
                        fileNameWithoutCS = ss[0];
                        cs = ss[1];
                    }

                    if (cs == "")
                    {
                        cs = "cs";
                    }

                    ExcelPackage p = GetPackage(Path.GetFullPath(path));

                    string protoName = fileNameWithoutCS;
                    if (fileNameWithoutCS.Contains('_'))
                    {
                        protoName = fileNameWithoutCS.Substring(0, fileNameWithoutCS.LastIndexOf('_'));
                    }

                    Table table = GetTable(protoName);

                    if (cs.Contains("c"))
                    {
                        table.C = true;
                    }
#if !NOT_SERVER
                    if (cs.Contains("s"))
                    {
                        table.S = true;
                    }
#endif
                    ExportExcelClass(p, protoName, table);
                }

                foreach (var kv in tables)
                {
                    if (kv.Value.C)
                    {
                        ExportClass(kv.Key, kv.Value.HeadInfos, ConfigType.c);
                    }
#if !NOT_SERVER
                    if (kv.Value.S)
                    {
                        ExportClass(kv.Key, kv.Value.HeadInfos, ConfigType.s);
                    }
#endif
                }

                // 动态编译生成的配置代码
                ExportArrayWrappers(ConfigType.c);
                configAssemblies[(int)ConfigType.c] = DynamicBuild(ConfigType.c);
#if !NOT_SERVER
                configAssemblies[(int)ConfigType.s] = DynamicBuild(ConfigType.s);
#endif

                //foreach (string path in ExportHelper.FindFile(excelDir))
                //{
                //    ExportExcel(path);
                //}

                // 多线程导出
                List<Task> tasks = new List<Task>();
                foreach (string path in ExportHelper.FindFile(excelDir))
                {
                    Task task = Task.Run(() => ExportExcel(path));
                    tasks.Add(task);
                }
                Task.WaitAll(tasks.ToArray());

                Console.WriteLine("ExcelExporter 成功");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            finally
            {
                tables.Clear();
                foreach (var kv in packages)
                {
                    kv.Value.Dispose();
                }

                packages.Clear();
            }
        }

        public static void ExportTarget(string target)
        {
            string fullAbsolute = Path.GetFullPath(target);
           
            var name = Path.GetFileName(target);
            Console.WriteLine($"Exporter{name} 开始");
            try
            {
                neededArrayWrappers.Clear();
                template = File.ReadAllText("Template.txt");
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                foreach (string path in ExportHelper.FindFile(excelDir))
                {
                    string fullRelative = Path.GetFullPath(path, Directory.GetCurrentDirectory());
                    bool ignoreCase = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
                    if (!string.Equals(fullRelative, fullAbsolute, ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal)) continue;
                    string fileName = Path.GetFileName(path);
                    if (!fileName.EndsWith(".xlsx") || fileName.StartsWith("~$") || fileName.Contains("#"))
                    {
                        continue;
                    }

                    string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
                    string fileNameWithoutCS = fileNameWithoutExtension;
                    string cs = "cs";
                    if (fileNameWithoutExtension.Contains("@"))
                    {
                        string[] ss = fileNameWithoutExtension.Split("@");
                        fileNameWithoutCS = ss[0];
                        cs = ss[1];
                    }

                    if (cs == "")
                    {
                        cs = "cs";
                    }

                    ExcelPackage p = GetPackage(Path.GetFullPath(path));

                    string protoName = fileNameWithoutCS;
                    if (fileNameWithoutCS.Contains('_'))
                    {
                        protoName = fileNameWithoutCS.Substring(0, fileNameWithoutCS.LastIndexOf('_'));
                    }

                    Table table = GetTable(protoName);

                    if (cs.Contains("c"))
                    {
                        table.C = true;
                    }
#if !NOT_SERVER
                    if (cs.Contains("s"))
                    {
                        table.S = true;
                    }
#endif
                    ExportExcelClass(p, protoName, table);
                }

                foreach (var kv in tables)
                {
                    if (kv.Value.C)
                    {
                        ExportClass(kv.Key, kv.Value.HeadInfos, ConfigType.c);
                    }
#if !NOT_SERVER
                    if (kv.Value.S)
                    {
                        ExportClass(kv.Key, kv.Value.HeadInfos, ConfigType.s);
                    }
#endif
                }

                // 动态编译生成的配置代码
                ExportArrayWrappers(ConfigType.c);
                configAssemblies[(int)ConfigType.c] = DynamicBuild(ConfigType.c);
#if !NOT_SERVER
                configAssemblies[(int)ConfigType.s] = DynamicBuild(ConfigType.s);
#endif

                //foreach (string path in ExportHelper.FindFile(excelDir))
                //{
                //    ExportExcel(path);
                //}

                // 多线程导出
                List<Task> tasks = new List<Task>();
                foreach (string path in ExportHelper.FindFile(excelDir))
                {
                    string fullRelative = Path.GetFullPath(path, Directory.GetCurrentDirectory());
                    bool ignoreCase = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
                    if (!string.Equals(fullRelative, fullAbsolute, ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal)) continue;
                    Task task = Task.Run(() => ExportExcel(path));
                    tasks.Add(task);
                }
                Task.WaitAll(tasks.ToArray());

                Console.WriteLine("ExcelExporterTarget 成功");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            finally
            {
                tables.Clear();
                foreach (var kv in packages)
                {
                    kv.Value.Dispose();
                }

                packages.Clear();
            }
        }

        private static void ExportExcel(string path)
        {
            string dir = Path.GetDirectoryName(path);
            string relativePath = Path.GetRelativePath(excelDir, dir);
            string fileName = Path.GetFileName(path);
            if (!fileName.EndsWith(".xlsx") || fileName.StartsWith("~$") || fileName.Contains("#"))
            {
                return;
            }

            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
            string fileNameWithoutCS = fileNameWithoutExtension;
            string cs = "cs";
            if (fileNameWithoutExtension.Contains("@"))
            {
                string[] ss = fileNameWithoutExtension.Split("@");
                fileNameWithoutCS = ss[0];
                cs = ss[1];
            }

            if (cs == "")
            {
                cs = "cs";
            }

            string protoName = fileNameWithoutCS;
            if (fileNameWithoutCS.Contains('_'))
            {
                protoName = fileNameWithoutCS.Substring(0, fileNameWithoutCS.LastIndexOf('_'));
            }

            Table table = GetTable(protoName);

            ExcelPackage p = GetPackage(Path.GetFullPath(path));

            if (cs.Contains("c"))
            {
                ExportExcelJson(p, fileNameWithoutCS, table, ConfigType.c, relativePath);
                ExportExcelProtobuf(ConfigType.c, protoName, relativePath);
            }

            if (cs.Contains("s"))
            {
                ExportExcelJson(p, fileNameWithoutCS, table, ConfigType.s, relativePath);
#if !NOT_SERVER
                ExportExcelProtobuf(ConfigType.s, protoName, relativePath);
#endif
            }

        }

        private static string GetProtoDir(ConfigType configType, string relativeDir)
        {
#if !NOT_SERVER
            if (configType == ConfigType.c)
            {
                return string.Format(clientProtoDir, ".");
            }

            return string.Format(serverProtoDir, relativeDir);
#else
            return string.Format(clientProtoDir, ".");
#endif
        }

        private static Assembly GetAssembly(ConfigType configType)
        {
            return configAssemblies[(int)configType];
        }

        private static string GetClassDir(ConfigType configType)
        {
#if !NOT_SERVER
            if (configType == ConfigType.c)
            {
                return ClientClassDir;
            }

            return ServerClassDir;
#else
            return ClientClassDir;
#endif
        }

        // 动态编译生成的cs代码
        private static Assembly DynamicBuild(ConfigType configType)
        {
            string classPath = GetClassDir(configType);
            List<SyntaxTree> syntaxTrees = new List<SyntaxTree>();
            List<string> protoNames = new List<string>();
            foreach (string classFile in Directory.GetFiles(classPath, "*.cs"))
            {
                protoNames.Add(Path.GetFileNameWithoutExtension(classFile));
                syntaxTrees.Add(CSharpSyntaxTree.ParseText(File.ReadAllText(classFile)));
            }

            List<PortableExecutableReference> references = new List<PortableExecutableReference>();
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly assembly in assemblies)
            {
                try
                {
                    if (assembly.IsDynamic)
                    {
                        continue;
                    }

                    if (assembly.Location == "")
                    {
                        continue;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }

                PortableExecutableReference reference = MetadataReference.CreateFromFile(assembly.Location);
                references.Add(reference);
            }

            CSharpCompilation compilation = CSharpCompilation.Create(null,
                syntaxTrees.ToArray(),
                references.ToArray(),
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            using MemoryStream memSteam = new MemoryStream();

            EmitResult emitResult = compilation.Emit(memSteam);
            if (!emitResult.Success)
            {
                StringBuilder stringBuilder = new StringBuilder();
                foreach (Diagnostic t in emitResult.Diagnostics)
                {
                    stringBuilder.AppendLine(t.GetMessage());
                }

                throw new Exception($"动态编译失败:\n{stringBuilder}");
            }

            memSteam.Seek(0, SeekOrigin.Begin);

            Assembly ass = Assembly.Load(memSteam.ToArray());
            return ass;
        }


#region 导出class

        static void ExportExcelClass(ExcelPackage p, string name, Table table)
        {
            foreach (ExcelWorksheet worksheet in p.Workbook.Worksheets)
            {
                try
                {
                    if (worksheet.Dimension == null || worksheet.Dimension.End == null) continue;
                    Console.WriteLine("ExportSheetClass " + name);
                    ExportSheetClass(worksheet, table);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(name + "--" + worksheet.Name + "     有错误 " + ex);
                }
            }
        }

        static void ExportSheetClass(ExcelWorksheet worksheet, Table table)
        {
            const int row = 2;
            for (int col = 3; col <= worksheet.Dimension.End.Column; ++col)
            {
                if (worksheet.Name.StartsWith("#"))
                {
                    continue;
                }

                string fieldName = worksheet.Cells[row + 2, col].Text.Trim();
                if (fieldName == "")
                {
                    continue;
                }

                if (table.HeadInfos.ContainsKey(fieldName))
                {
                    continue;
                }

                string fieldCS = worksheet.Cells[row, col].Text.Trim().ToLower();
                if (fieldCS.Contains("#"))
                {
                    table.HeadInfos[fieldName] = null;
                    continue;
                }

                if (fieldCS == "")
                {
                    fieldCS = "cs";
                }

                if (table.HeadInfos.TryGetValue(fieldName, out var oldClassField))
                {
                    if (oldClassField.FieldAttribute != fieldCS)
                    {
                        Console.WriteLine($"field cs not same: {worksheet.Name} {fieldName} oldcs: {oldClassField.FieldAttribute} {fieldCS}");
                    }

                    continue;
                }

                string fieldDesc = worksheet.Cells[row + 1, col].Text.Trim();
                string fieldType = worksheet.Cells[row + 3, col].Text.Trim();

                table.HeadInfos[fieldName] = new HeadInfo(fieldCS, fieldDesc, fieldName, fieldType, ++table.Index);
            }
        }

        static void ExportClass(string protoName, Dictionary<string, HeadInfo> classField, ConfigType configType)
        {
            string dir = GetClassDir(configType);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            string exportPath = Path.Combine(dir, $"{protoName}.cs");

            using FileStream txt = new FileStream(exportPath, FileMode.Create);
            using StreamWriter sw = new StreamWriter(txt);

            StringBuilder sb = new StringBuilder();
            foreach ((string _, HeadInfo headInfo) in classField)
            {
                if (headInfo == null)
                {
                    continue;
                }

                if (headInfo.FieldType == "json")
                {
                    continue;
                }

                if (!headInfo.FieldAttribute.Contains(configType.ToString()))
                {
                    continue;
                }
                string fieldType = ConvertType(headInfo.FieldType);
                sb.Append($"\t\t/// <summary>{headInfo.FieldDesc.Replace("\n", "</summary>\n\t\t/// <summary> ")}</summary>\n");
                sb.Append($"\t\t[ProtoMember({headInfo.FieldIndex})]\n");
                

                sb.Append($"\t\tpublic {fieldType} {headInfo.FieldName} {{ get; set; }}\n");
            }

            string content = template.Replace("(ConfigName)", protoName).Replace(("(Fields)"), sb.ToString());
            sw.Write(content);
        }

#endregion

#region 导出json


        static void ExportExcelJson(ExcelPackage p, string name, Table table, ConfigType configType, string relativeDir)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("{\"list\":[");
            foreach (ExcelWorksheet worksheet in p.Workbook.Worksheets)
            {
                if (worksheet.Name.StartsWith("#"))
                {
                    continue;
                }
                if (worksheet.Dimension == null || worksheet.Dimension.End == null) continue;
                Console.WriteLine("ExportExcelJson " + name);
                ExportSheetJson(worksheet, name, table.HeadInfos, configType, sb);
            }

            sb.AppendLine("]}");

            string dir = string.Format(jsonDir, configType.ToString(), relativeDir);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            string jsonPath = Path.Combine(dir, $"{name}.txt");
            using FileStream txt = new FileStream(jsonPath, FileMode.Create);
            using StreamWriter sw = new StreamWriter(txt);
            sw.Write(sb.ToString());
        }

        static void ExportSheetJson(ExcelWorksheet worksheet, string name,
                Dictionary<string, HeadInfo> classField, ConfigType configType, StringBuilder sb)
        {
            string configTypeStr = configType.ToString();
            for (int row = 6; row <= worksheet.Dimension.End.Row; ++row)
            {
                string prefix = worksheet.Cells[row, 2].Text.Trim();
                if (prefix.Contains("#"))
                {
                    continue;
                }

                if (prefix == "")
                {
                    prefix = "cs";
                }

                if (!prefix.Contains(configTypeStr))
                {
                    continue;
                }

                if (worksheet.Cells[row, 3].Text.Trim() == "")
                {
                    continue;
                }

                sb.Append("{");
                sb.Append($"\"_t\":\"{name}\"");
                for (int col = 3; col <= worksheet.Dimension.End.Column; ++col)
                {
                    string fieldName = worksheet.Cells[4, col].Text.Trim();
                    if (!classField.ContainsKey(fieldName))
                    {
                        continue;
                    }

                    HeadInfo headInfo = classField[fieldName];

                    if (headInfo == null)
                    {
                        continue;
                    }

                    if (!headInfo.FieldAttribute.Contains(configTypeStr))
                    {
                        continue;
                    }

                    if (headInfo.FieldType == "json")
                    {
                        continue;
                    }

                    string fieldN = headInfo.FieldName;

                    sb.Append($",\"{fieldN}\":{Convert(headInfo.FieldType, worksheet.Cells[row, col].Text.Trim())}");
                }

                sb.Append("},\n");
            }
        }

        private static string GetArrayWrapperName(string elementType)
        {
            switch (elementType)
            {
                case "int":
                case "int32":
                    return "IntArray";
                case "long":
                case "int64":
                    return "LongArray";
                case "uint":
                    return "UIntArray";
                case "ulong":
                    return "ULongArray";
                case "float":
                    return "FloatArray";
                case "double":
                    return "DoubleArray";
                case "decimal":
                    return "DecimalArray";
                default:
                    throw new Exception($"不支持此锯齿数组元素类型: {elementType}");
            }
        }

        private static string GetArrayWrapperElementType(string wrapperName)
        {
            switch (wrapperName)
            {
                case "IntArray": return "int";
                case "LongArray": return "long";
                case "UIntArray": return "uint";
                case "ULongArray": return "ulong";
                case "FloatArray": return "float";
                case "DoubleArray": return "double";
                case "DecimalArray": return "decimal";
                default: throw new Exception($"Unknown array wrapper: {wrapperName}");
            }
        }

        private static string ConvertJaggedArray(string type, string value)
        {
            string elementType = type.Substring(0, type.Length - 4);
            string wrapperName = GetArrayWrapperName(elementType);

            value = value.Replace("{", "[").Replace("}", "]");
            value = value.Trim();
            if (value.StartsWith("[")) value = value.Substring(1);
            if (value.EndsWith("]")) value = value.Substring(0, value.Length - 1);
            value = value.Trim();

            if (string.IsNullOrEmpty(value)) return "[]";

            string[] subArrays = value.Split(new[] { "],[" }, StringSplitOptions.None);
            StringBuilder sb = new StringBuilder();
            sb.Append("[");
            for (int i = 0; i < subArrays.Length; i++)
            {
                string arr = subArrays[i].Trim().Trim('[', ']');
                sb.Append($"{{\"_t\":\"{wrapperName}\",\"items\":[{arr}]}}");
                if (i < subArrays.Length - 1) sb.Append(",");
            }
            sb.Append("]");
            return sb.ToString();
        }

        private static void ExportArrayWrappers(ConfigType configType)
        {
            if (neededArrayWrappers.Count == 0) return;
            string dir = GetClassDir(configType);
            string exportPath = Path.Combine(dir, "ArrayWrappers.cs");

            StringBuilder sb = new StringBuilder();
            sb.Append("using ProtoBuf;\n");
            sb.Append("namespace TaoTie\n{\n");
            foreach (string wrapperName in neededArrayWrappers)
            {
                string elementType = GetArrayWrapperElementType(wrapperName);
                sb.Append("\t[ProtoContract]\n");
                sb.Append($"\tpublic partial class {wrapperName} : ProtoObject\n");
                sb.Append("\t{\n");
                sb.Append("\t\t[ProtoMember(1)]\n");
                sb.Append($"\t\tpublic {elementType}[] items {{ get; set; }} = new {elementType}[0];\n");
                sb.Append("\t}\n");
            }
            sb.Append("}\n");

            File.WriteAllText(exportPath, sb.ToString());
        }

        private static string Convert(string type, string value)
        {
            switch (type)
            {
                case "decimal[]":
                case "double[]":
                case "uint[]":
                case "int[]":
                case "int32[]":
                case "long[]":
                case "ulong[]":
                case "float[]":
                    {
                        value = value.Replace("{", "").Replace("}", "");
                        return $"[{value}]";
                    }
                case "string[]":
                case "BigInteger[]":
                    if (string.IsNullOrEmpty(value)) return "[]";
                    if (value.StartsWith("\""))
                    {
                        return $"[{value}]";
                    }
                    var list = value.Split(",");
                    value = "";
                    for (int i = 0; i < list.Length; i++)
                    {
                        value += "\"" + list[i] + "\"";
                        if (i < list.Length - 1) value += ",";
                    }
                    return $"[{value}]";
                case "decimal[][]":
                case "double[][]":
                case "uint[][]":
                case "int[][]":
                case "int32[][]":
                case "long[][]":
                case "ulong[][]":
                case "float[][]":
                    return ConvertJaggedArray(type, value);
                case "int":
                case "uint":
                case "int32":
                case "int64":
                case "long":
                case "float":
                case "double":
                    {
                        value = value.Replace("{", "").Replace("}", "");
                        if (value == "")
                        {
                            return "0";
                        }
                        return value;
                    }
                case "string":
                    return $"\"{value}\"";
                case "BigInteger":
                    value = value.Replace("{", "").Replace("}", "");
                    if (value == "")
                    {
                        value = "0";
                    }
                    return $"\"{value}\"";
                case "AttrConfig":
                    string[] ss = value.Split(':');
                    return "{\"_t\":\"AttrConfig\"," + "\"Ks\":" + ss[0] + ",\"Vs\":" + ss[1] + "}";
                default:
                    throw new Exception($"不支持此类型: {type}");
            }
        }
        private static string ConvertType(string type)
        {
            if (type.EndsWith("[][]"))
            {
                string elementType = type.Substring(0, type.Length - 4);
                string wrapperName = GetArrayWrapperName(elementType);
                neededArrayWrappers.Add(wrapperName);
                return $"{wrapperName}[]";
            }
            return type;
        }
        #endregion


        // 根据生成的类，把json转成protobuf
        private static void ExportExcelProtobuf(ConfigType configType, string protoName, string relativeDir)
        {
            string dir = GetProtoDir(configType, relativeDir);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            Assembly ass = GetAssembly(configType);
            Type type = ass.GetType($"TaoTie.{protoName}Category");
            Type subType = ass.GetType($"TaoTie.{protoName}");

            Serializer.NonGeneric.PrepareSerializer(type);
            Serializer.NonGeneric.PrepareSerializer(subType);

            IMerge final = Activator.CreateInstance(type) as IMerge;

            string p = Path.Combine(string.Format(jsonDir, configType, relativeDir));
            string[] ss = Directory.GetFiles(p, $"{protoName}_*.txt");
            List<string> jsonPaths = ss.ToList();
            jsonPaths.Add(Path.Combine(string.Format(jsonDir, configType, relativeDir), $"{protoName}.txt"));

            jsonPaths.Sort();
            jsonPaths.Reverse();
            FieldInfo listField = type.GetField("list", BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (string jsonPath in jsonPaths)
            {
                string json = File.ReadAllText(jsonPath);
                JsonData jsonData = JsonMapper.ToObject(json);
                object deserialize = Activator.CreateInstance(type);
                System.Collections.IList list = (System.Collections.IList)listField.GetValue(deserialize);
                foreach (JsonData item in jsonData["list"])
                {
                    list.Add(JsonMapper.ToObject(subType, item.ToJson()));
                }
                final.Merge(deserialize);
            }

            string path = Path.Combine(dir, $"{protoName}Category.bytes");

            using FileStream file = File.Create(path);
            Serializer.Serialize(file, final);
        }
    }
}