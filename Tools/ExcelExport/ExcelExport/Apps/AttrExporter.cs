using System;
using System.IO;
using System.Text;
using OfficeOpenXml;
using System.Collections.Generic;
using System.Runtime;

namespace TaoTie
{
    internal class AttrExporter
    {
        private const string ClassDir = "../Assets/Scripts/Code/Module/Const/NumericType.cs";

        private const string excelDir = "../Excel";
        public class Info
        {
            public string Key;
            public string Id;
            public string Remarks;
            public string Affected;
        }

        public static void Export()
        {
            Console.WriteLine("AttrExporter 开始");
            foreach (string excelPath in ExportHelper.FindFile(excelDir))
            {
                if (!excelPath.EndsWith(".xlsx") || excelPath.StartsWith("~$") || excelPath.Contains("#"))
                {
                    continue;
                }
                if (!excelPath.Contains("AttributeConfig")) continue;
                using Stream stream = new FileStream(excelPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using ExcelPackage p = new ExcelPackage(stream);

                List<Info> list = new List<Info>();
                ExportExcelEnum(p, list);

                StringBuilder str = new StringBuilder();
                str.AppendLine("using System.Collections.Generic;");
                str.AppendLine("namespace TaoTie");
                str.AppendLine("{");
                str.AppendLine("    public class NumericType");
                str.AppendLine("    {");
                str.AppendLine("        public int this[string key]");
                str.AppendLine("        {");
                str.AppendLine("            get");
                str.AppendLine("            {");
                str.AppendLine("                if (Map.TryGetValue(key, out var res))");
                str.AppendLine("                {");
                str.AppendLine("                    return res;");
                str.AppendLine("                }");
                str.AppendLine("                Log.Error($\"{key}属性不存在\");");
                str.AppendLine("                return -1;");
                str.AppendLine("            }");
                str.AppendLine("        }");
                str.AppendLine("        private static Dictionary<string, int> __Map;");
                str.AppendLine("        public static Dictionary<string, int> Map");
                str.AppendLine("        {");
                str.AppendLine("            get");
                str.AppendLine("            {");
                str.AppendLine("                if (__Map == null)");
                str.AppendLine("                {");
                str.AppendLine("                    __Map = new Dictionary<string, int>();");
                foreach (var item in list)
                {
                    str.AppendLine($"                    __Map.Add(\"{item.Key}\",{item.Key});");
                    str.AppendLine($"                    __Map.Add(\"{item.Key}Base\",{item.Key}Base);");
                    if (item.Affected == "1")
                    {
                        str.AppendLine($"                    __Map.Add(\"{item.Key}Add\",{item.Key}Add);");
                        str.AppendLine($"                    __Map.Add(\"{item.Key}Pct\",{item.Key}Pct);");
                        str.AppendLine($"                    __Map.Add(\"{item.Key}FinalAdd\",{item.Key}FinalAdd);");
                        str.AppendLine($"                    __Map.Add(\"{item.Key}FinalPct\",{item.Key}FinalPct);");
                    }
                }

                str.AppendLine("                }");
                str.AppendLine("                return __Map;");
                str.AppendLine("            }");
                str.AppendLine("        }");
                str.AppendLine("		public const int Max = 10000;");

                foreach (var item in list)
                {

                    str.AppendLine();
                    str.AppendLine($"		/// <summary> {item.Remarks} </summary>");
                    str.AppendLine($"		public const int {item.Key} = {item.Id};");
                    str.AppendLine($"		/// <summary> {item.Remarks}Base </summary>");
                    str.AppendLine($"		public const int {item.Key}Base = {item.Id} * 10 + 1;");
                    if (item.Affected == "1")
                    {
                        str.AppendLine($"		/// <summary> {item.Remarks}Add </summary>");
                        str.AppendLine($"		public const int {item.Key}Add = {item.Id} * 10 + 2;");
                        str.AppendLine($"		/// <summary> {item.Remarks}Pct </summary>");
                        str.AppendLine($"		public const int {item.Key}Pct = {item.Id} * 10 + 3;");
                        str.AppendLine($"		/// <summary> {item.Remarks}FinalAdd </summary>");
                        str.AppendLine($"		public const int {item.Key}FinalAdd = {item.Id} * 10 + 4;");
                        str.AppendLine($"		/// <summary> {item.Remarks}FinalPct </summary>");
                        str.AppendLine($"		public const int {item.Key}FinalPct = {item.Id} * 10 + 5;");
                    }
                }

                str.AppendLine("    }");
                str.AppendLine("}");

                File.WriteAllText(ClassDir, str.ToString());
                Console.WriteLine("AttrExporter 成功");
                return;
            }
            Console.WriteLine("AttrExporter 文件未找到");
        }

        static void ExportExcelEnum(ExcelPackage p, List<Info> list)
        {
            foreach (ExcelWorksheet worksheet in p.Workbook.Worksheets)
            {
                if (worksheet.Dimension == null || worksheet.Dimension.End == null) continue;
                ExportSheetEnum(worksheet, list);
            }
        }

        static void ExportSheetEnum(ExcelWorksheet worksheet, List<Info> sb)
        {
            int infoRow = 2;
            int KeyIndex = 0, RemarksIndex = 0, AffectedIndex = 0, IdIndex = 0;
            for (int col = 3; col <= worksheet.Dimension.End.Column; ++col)
            {
                string fieldName = worksheet.Cells[infoRow + 2, col].Text.Trim();
                if (fieldName == "")
                {
                    continue;
                }
                if (fieldName == "Id")
                {
                    IdIndex = col;
                    continue;
                }
                if (fieldName == "Remarks")
                {
                    RemarksIndex = col;
                    continue;
                }
                if (fieldName == "Key")
                {
                    KeyIndex = col;
                    continue;
                }
                if (fieldName == "Affected")
                {
                    AffectedIndex = col;
                    continue;
                }
            }

            for (int row = 6; row <= worksheet.Dimension.End.Row; ++row)
            {
                Info info = new Info();

                if (worksheet.Cells[row, 3].Text.Trim() == "")
                {
                    continue;
                }

                info.Id = worksheet.Cells[row, IdIndex].Text;
                info.Remarks = worksheet.Cells[row, RemarksIndex].Text;
                info.Key = worksheet.Cells[row, KeyIndex].Text;
                info.Affected = worksheet.Cells[row, AffectedIndex].Text;
                sb.Add(info);
            }
        }

    }
}
