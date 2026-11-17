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

                // 不再整目录删除 class / proto 输出目录：产物旁边跟着 Unity 的 .meta，里面存着资源 uuid，
                // 连目录一起删会让编辑器重新导入时分配新 uuid，已保存的 prefab / scene / 代码引用全部断开。
                // 改为扫完表之后，按"本次会生成的文件名"清掉过期产物，其余原样覆盖（见 CleanStaleOutputs）。
                string cProtoDir = string.Format(clientProtoDir, ".");

                // 中间 json 目录仍必须清空，否则已删除/改名的表残留旧文件，会污染 proto 全局合并与 server 输出。
                // 这里是 Excel/Json 下的纯中间产物（不进 Assets、无 .meta），整目录删除是安全的。
                foreach (string ct in new[] { "c", "s" })
                {
                    string jdir = string.Format(jsonDir, ct, "");
                    if (Directory.Exists(jdir))
                    {
                        Directory.Delete(jdir, true);
                    }
                }

                // 收集表头：按文件顺序串行遍历，HeadInfos 的插入顺序决定字段顺序与字段号，不能并行。
                foreach (string path in ExportHelper.FindFile(excelDir))
                {
                    ExportJob job = BuildJob(path);
                    if (job == null)
                    {
                        continue;
                    }

                    Table table = GetTable(job.ProtoName);
                    if (job.HasC)
                    {
                        table.C = true;
                    }
#if !NOT_SERVER
                    if (job.HasS)
                    {
                        table.S = true;
                    }
#endif
                    ExportExcelClass(GetPackage(Path.GetFullPath(job.Path)), job.ProtoName, table);
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

                // 清理过期产物：只删"本次不会再生成"的 .cs / xxxCategory.bytes，其余文件原样覆盖，.meta 一律不动。
                // 表被删除或改名时旧的 bin / cs 才不会残留，同时保住 Unity 的 uuid 不被打乱。
                HashSet<string> expectedClass = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "ArrayWrappers.cs" };
                HashSet<string> expectedProto = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var kv in tables)
                {
                    if (kv.Value.C)
                    {
                        expectedClass.Add($"{kv.Key}.cs");
                        expectedProto.Add($"{kv.Key}Category.bytes");
                    }
#if !NOT_SERVER
                    if (kv.Value.S)
                    {
                        expectedClass.Add($"{kv.Key}.cs");
                    }
#endif
                }

                // 动态编译生成的配置代码（ArrayWrappers.cs 先写出来，再按目录内容编译）
                ExportArrayWrappers(ConfigType.c);
                CleanStaleOutputs(ClientClassDir, "*.cs", expectedClass);
#if !NOT_SERVER
                CleanStaleOutputs(ServerClassDir, "*.cs", expectedClass);
#endif
                CleanStaleOutputs(cProtoDir, "*Category.bytes", expectedProto);
                configAssemblies[(int)ConfigType.c] = DynamicBuild(ConfigType.c);
#if !NOT_SERVER
                configAssemblies[(int)ConfigType.s] = DynamicBuild(ConfigType.s);
#endif

                // 导出拆成两阶段：
                //   阶段一 并行，只把每个分表写成 Excel/Json/{c,s}/**/{name}.txt（不碰 bin、不碰服务端产物）；
                //   阶段二 串行，按输出目标合并，同一个产物文件只写一次。
                // 阶段一并行是安全的：各分表写各自的 json 路径，不共享输出文件；package 与输出目录都在并行前预热/预建。
                // 阶段二必须在所有 json 落盘之后，否则扫描不到同组其它分表会丢行。
                List<ExportJob> jobs = new List<ExportJob>();
                foreach (string path in ExportHelper.FindFile(excelDir))
                {
                    ExportJob job = BuildJob(path);
                    if (job == null) continue;
                    // 预热 package 缓存：packages 是普通 Dictionary，不是线程安全的；EPPlus 打开文件本身也应串行。
                    GetPackage(Path.GetFullPath(job.Path));
                    // 预建输出目录，免得并行阶段多个线程同时 CreateDirectory
                    if (job.HasC) EnsureJsonDir(ConfigType.c, job.RelativeDir);
                    if (job.HasS) EnsureJsonDir(ConfigType.s, job.RelativeDir);
                    jobs.Add(job);
                }

                List<string> jobErrors = new List<string>();
                Parallel.ForEach(jobs, job =>
                {
                    try
                    {
                        ExportExcelJsonOnly(job);
                    }
                    catch (Exception ex)
                    {
                        lock (jobErrors)
                        {
                            jobErrors.Add($"{Path.GetFileName(job.Path)}: {ex.Message}");
                        }
                    }
                });
                foreach (string err in jobErrors)
                {
                    Console.WriteLine("导出中间 json 失败 " + err);
                }

                // 阶段二：串行合并（合并顺序由 jsonPaths 排序决定，与阶段一的完成顺序无关）
                foreach (MergeAction m in BuildMergeActions(jobs))
                {
                    MergeProto(m.ConfigType, m.ProtoName, m.RelativeDir);
                }

                if (jobErrors.Count == 0)
                {
                    Console.WriteLine("ExcelExporter 成功");
                }
                else
                {
                    Console.WriteLine($"ExcelExporter 失败 {jobErrors.Count} 个分表（见上方日志），其余表已导出");
                }
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

        public static bool ExportTarget(string target)
        {
            string fullAbsolute = Path.GetFullPath(target);

            var name = Path.GetFileName(target);
            Console.WriteLine($"Exporter{name} 开始");
            try
            {
                neededArrayWrappers.Clear();
                template = File.ReadAllText("Template.txt");
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                bool ignoreCase = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
                StringComparison pathCompare = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

                // 1. 先定位目标文件，并解析出它的导出任务。找不到就明确报错，
                //    不再像原来那样什么都没导也打印"成功"（路径写法不对时会被误当成已导出）。
                ExportJob targetJob = null;
                foreach (string path in ExportHelper.FindFile(excelDir))
                {
                    if (!string.Equals(Path.GetFullPath(path, Directory.GetCurrentDirectory()), fullAbsolute, pathCompare)) continue;
                    targetJob = BuildJob(path);
                    break;
                }

                if (targetJob == null)
                {
                    Console.WriteLine($"ExcelExporterTarget 失败：{Path.GetFullPath(excelDir)} 下找不到可导出的目标表 {target}");
                    return false;
                }

                // 2. 表头必须按整个 protoName 分组收集，而不是只读目标这一张表。
                //    分表（ItemConfig_武器 / ItemConfig_头盔 …）共用同一个 ItemConfig 协议，
                //    只读其中一张会漏掉其它分表独有的字段（例如 param0），结果是生成的 .cs 缺字段、
                //    合并出的 bytes 把这些字段整列丢掉。这里复用全量导表的遍历顺序，
                //    字段 desc / @ProtoMember 序号才与全量导表结果一致，避免无意义的 .cs 抖动。
                List<ExportJob> groupJobs = new List<ExportJob>();
                foreach (string path in ExportHelper.FindFile(excelDir))
                {
                    ExportJob job = BuildJob(path);
                    if (job == null || !string.Equals(job.ProtoName, targetJob.ProtoName, StringComparison.Ordinal)) continue;

                    Table table = GetTable(job.ProtoName);
                    if (job.HasC)
                    {
                        table.C = true;
                    }
#if !NOT_SERVER
                    if (job.HasS)
                    {
                        table.S = true;
                    }
#endif
                    ExportExcelClass(GetPackage(Path.GetFullPath(job.Path)), job.ProtoName, table);
                    groupJobs.Add(job);
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

                // 3. 先写中间 json：同组的分表一起写。合并只能基于磁盘上的中间 json，
                //    只刷新目标分表时，其它分表的 json 可能是上次全量导表留下的旧数据，
                //    甚至从头就没生成过（Excel/Json 是会被清空的），那样合并出来的 bytes 会静默缺行。
                //    分表 package 在上一步收集表头时已经打开，这里不会额外付打开成本。
                foreach (ExportJob job in groupJobs)
                {
                    ExportExcelJsonOnly(job);
                }

                // 4. 合并按输出目标各做一次，与全量导表的两阶段保持一致。
                foreach (MergeAction m in BuildMergeActions(groupJobs))
                {
                    MergeProto(m.ConfigType, m.ProtoName, m.RelativeDir);
                }

                Console.WriteLine($"ExcelExporterTarget 成功：{targetJob.ProtoName}（{groupJobs.Count} 个分表）");
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return false;
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

        /// <summary>
        /// 单个分表的导出任务。同 protoName 的多个分表共享一个 xxxCategory.bytes / 服务端 json，
        /// 因此合并必须按输出目标归组、且每组只做一次。
        /// </summary>
        private sealed class ExportJob
        {
            /// <summary>源 xlsx 路径</summary>
            public string Path;
            /// <summary>相对 excelDir 的子目录，决定中间 json 落在哪一层</summary>
            public string RelativeDir;
            /// <summary>中间 json 文件名（分表保留 _标签，用于按 protoName 前缀合并）</summary>
            public string JsonName;
            /// <summary>协议名 / 合并键</summary>
            public string ProtoName;
            public bool HasC;
            public bool HasS;
        }

        /// <summary>阶段二的一次合并动作</summary>
        private sealed class MergeAction
        {
            public ConfigType ConfigType;
            public string ProtoName;
            public string RelativeDir;
        }

        /// <summary>解析文件名得到导出任务；不参与导出的文件返回 null</summary>
        private static ExportJob BuildJob(string path)
        {
            string fileName = Path.GetFileName(path);
            if (!fileName.EndsWith(".xlsx") || fileName.StartsWith("~$") || fileName.Contains("#"))
            {
                return null;
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

            return new ExportJob
            {
                Path = path,
                RelativeDir = Path.GetRelativePath(excelDir, Path.GetDirectoryName(path)),
                JsonName = fileNameWithoutCS,
                ProtoName = protoName,
                HasC = cs.Contains("c"),
                HasS = cs.Contains("s"),
            };
        }

        /// <summary>
        /// 按"输出目标"归并合并动作，保证同一个产物文件只写一次。
        /// 这里的合并函数按 jsonDir/{类型}/{relativeDir} 扫描中间 json，所以 protoName 与 relativeDir 要一起作为键。
        /// </summary>
        private static List<MergeAction> BuildMergeActions(List<ExportJob> jobs)
        {
            List<MergeAction> merges = new List<MergeAction>();
            HashSet<string> seen = new HashSet<string>();
            foreach (ExportJob job in jobs)
            {
                if (job.HasC && seen.Add("c|" + job.ProtoName + "|" + job.RelativeDir))
                {
                    merges.Add(new MergeAction { ConfigType = ConfigType.c, ProtoName = job.ProtoName, RelativeDir = job.RelativeDir });
                }
#if !NOT_SERVER
                if (job.HasS && seen.Add("s|" + job.ProtoName + "|" + job.RelativeDir))
                {
                    merges.Add(new MergeAction { ConfigType = ConfigType.s, ProtoName = job.ProtoName, RelativeDir = job.RelativeDir });
                }
#endif
            }

            return merges;
        }

        private static void EnsureJsonDir(ConfigType configType, string relativeDir)
        {
            string dir = string.Format(jsonDir, configType.ToString(), relativeDir);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
        }

        /// <summary>
        /// 删除目录里本次不会再生成的旧产物，**.meta 一律不动**。
        /// 不能整目录删除的原因：Unity 的 .meta 记着资源 uuid，连目录一起删会让编辑器重新导入时
        /// 分配新 uuid，已保存的 prefab / scene / 代码引用会全部断开。
        /// </summary>
        private static void CleanStaleOutputs(string dir, string searchPattern, HashSet<string> expectedNames)
        {
            if (!Directory.Exists(dir))
            {
                return;
            }

            foreach (string file in Directory.GetFiles(dir, searchPattern, SearchOption.TopDirectoryOnly))
            {
                if (file.EndsWith(".meta", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                string name = Path.GetFileName(file);
                if (expectedNames.Contains(name))
                {
                    continue;
                }

                try
                {
                    File.Delete(file);
                    Console.WriteLine("清理过期产物 " + name);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"清理过期产物失败 {name}: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// 阶段一：只把单个分表写成中间 json，不写 bin / 不写服务端合并产物，可与其它分表并行。
        /// </summary>
        private static void ExportExcelJsonOnly(ExportJob job)
        {
            Table table = GetTable(job.ProtoName);
            ExcelPackage p = GetPackage(Path.GetFullPath(job.Path));

            if (job.HasC)
            {
                ExportExcelJson(p, job.JsonName, table, ConfigType.c, job.RelativeDir);
            }

#if !NOT_SERVER
            if (job.HasS)
            {
                ExportExcelJson(p, job.JsonName, table, ConfigType.s, job.RelativeDir);
            }
#endif
        }

        /// <summary>阶段二：把一个 protoName 的全部中间 json 合并成一份产物。必须在阶段一全部落盘后调用。</summary>
        private static void MergeProto(ConfigType configType, string protoName, string relativeDir)
        {
            ExportExcelProtobuf(configType, protoName, relativeDir);
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