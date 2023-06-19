#define NOT_SERVER //导服务端配置开关
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using MongoDB.Bson.Serialization;
using OfficeOpenXml;
using Nino.Serialization;
using LicenseContext = OfficeOpenXml.LicenseContext;

namespace TaoTie
{
    public enum ConfigType
    {
        c = 0,
#if !NOT_SERVER
        s = 1,
#endif
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
    [NinoSerialize()]
    class Table
    {
        [NinoMember(1)]
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
                        ExportClass(kv.Key, kv.Value.HeadInfos, ConfigType.c, true);
                    }
#if !NOT_SERVER
                    if (kv.Value.S)
                    {
                        ExportClass(kv.Key, kv.Value.HeadInfos, ConfigType.s, true);
                    }
#endif
                }

                // 动态编译生成的配置代码
                configAssemblies[(int)ConfigType.c] = DynamicBuild(ConfigType.c);
#if !NOT_SERVER
                configAssemblies[(int)ConfigType.s] = DynamicBuild(ConfigType.s);
#endif
                foreach (var kv in tables)
                {
                    if (kv.Value.C)
                    {
                        ExportClass(kv.Key, kv.Value.HeadInfos, ConfigType.c);
                    }
                }
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
#if !NOT_SERVER
            if (cs.Contains("s"))
            {
                ExportExcelJson(p, fileNameWithoutCS, table, ConfigType.s, relativePath);
                ExportExcelProtobuf(ConfigType.s, protoName, relativePath);
            }
#endif
        }

        private static string GetProtoDir(ConfigType configType, string relativeDir)
        {
#if !NOT_SERVER
            if (configType == ConfigType.c || configType == ConfigType.p)
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

        static void ExportClass(string protoName, Dictionary<string, HeadInfo> classField, ConfigType configType, bool setattr = false)
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
                if (setattr && headInfo.FieldType.IndexOf("float", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    sb.Append("\t\t[BsonRepresentation(MongoDB.Bson.BsonType.Double, AllowTruncation = true)]\n");
                }
                sb.Append($"\t\t/// <summary>{headInfo.FieldDesc.Replace("\n", "</summary>\n\t\t/// <summary> ")}</summary>\n");
                sb.Append($"\t\t[NinoMember({headInfo.FieldIndex})]\n");
                string fieldType = headInfo.FieldType;
                if (fieldType == "int[][]")
                {
                    fieldType = "string[]";
                }

                sb.Append($"\t\tpublic {fieldType} {headInfo.FieldName} {{ get; set; }}\n");
            }

            string content = template.Replace("(ConfigName)", protoName).Replace(("(Fields)"), sb.ToString());
            if (!setattr)
            {
                content = content.Replace("[BsonIgnore]", "");
                content = content.Replace("[BsonElement]", "");
                content = content.Replace("using MongoDB.Bson.Serialization.Attributes;", "");
            }
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
                    if (fieldN == "Id")
                    {
                        fieldN = "_id";
                    }

                    sb.Append($",\"{fieldN}\":{Convert(headInfo.FieldType, worksheet.Cells[row, col].Text.Trim())}");
                }

                sb.Append("},\n");
            }
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
                case "float[]":
                    {
                        value = value.Replace("{", "").Replace("}", "");
                        return $"[{value}]";
                    }
                case "string[]":
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
                case "int[][]":
                    return $"[{value}]";
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
                case "AttrConfig":
                    string[] ss = value.Split(':');
                    return "{\"_t\":\"AttrConfig\"," + "\"Ks\":" + ss[0] + ",\"Vs\":" + ss[1] + "}";
                default:
                    throw new Exception($"不支持此类型: {type}");
            }
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
            IMerge final = Activator.CreateInstance(type) as IMerge;

            string p = Path.Combine(string.Format(jsonDir, configType, relativeDir));
            string[] ss = Directory.GetFiles(p, $"{protoName}_*.txt");
            List<string> jsonPaths = ss.ToList();
            jsonPaths.Add(Path.Combine(string.Format(jsonDir, configType, relativeDir), $"{protoName}.txt"));

            jsonPaths.Sort();
            jsonPaths.Reverse();
            foreach (string jsonPath in jsonPaths)
            {
                string json = File.ReadAllText(jsonPath);
                object deserialize = BsonSerializer.Deserialize(json, type);
                final.Merge(deserialize);
            }

            object data = final;
            string path = Path.Combine(dir, $"{protoName}Category.bytes");
            var bytes = Serializer.Serialize(data);
            using FileStream file = File.Create(path);
            file.Write(bytes);
        }
    }
}