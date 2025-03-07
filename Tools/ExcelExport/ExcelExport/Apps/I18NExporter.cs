using System.Text;
using Nino.Serialization;
using OfficeOpenXml;

namespace TaoTie
{
    public partial class ExcelExporter
    {
        private const string ClassDir = "../Assets/Scripts/Code/Module/Const/LangType.cs";
        private const string ClassDir2 = "../Assets/Scripts/Code/Module/Const/I18NKey.cs";
        public static void ExportI18N()
        {
            Console.WriteLine("I18NExporter 开始");
            Dictionary<string, I18NConfigCategory> i18nconfig = new Dictionary<string, I18NConfigCategory>();
            foreach (string excelPath in ExportHelper.FindFile(excelDir))
            {
                string dir = Path.GetDirectoryName(excelPath);
                string relativePath = Path.GetRelativePath(excelDir, dir);
                string fileName = Path.GetFileName(excelPath);
                if (!fileName.EndsWith(".xlsx") || fileName.StartsWith("~$") || fileName.Contains("#"))
                {
                    continue;
                }

                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);

                string cs = "cs";
                if (fileNameWithoutExtension.Contains("@"))
                {
                    string[] ss = fileNameWithoutExtension.Split("@");
                    cs = ss[1];
                }

                if (cs!="i")
                {
                    continue;
                }

                ExcelPackage p = GetPackage(Path.GetFullPath(excelPath));
                ExportExcelI18N(p, i18nconfig);
                ExportI18NExcelProtobuf(i18nconfig, relativePath);
                StringBuilder str = new StringBuilder();
                str.AppendLine("using System.Collections.Generic;");
                str.AppendLine("namespace TaoTie");
                str.AppendLine("{");
                str.AppendLine("    public enum I18NKey");
                str.AppendLine("    {");
                foreach (var item in i18nconfig)
                {
                    foreach (var item2 in item.Value.GetAllList()) 
                    {
                        str.AppendLine($"        {item2.Key} = {item2.Id},");
                    }
                   
                    break;
                }
                str.AppendLine("    }");
                str.AppendLine("}");
                File.WriteAllText(ClassDir2, str.ToString());
            }
            Console.WriteLine("I18NExporter 成功");
        }
        
        static void ExportExcelI18N(ExcelPackage p, Dictionary<string, I18NConfigCategory> list)
        {
            foreach (ExcelWorksheet worksheet in p.Workbook.Worksheets)
            {
                if (worksheet.Name.StartsWith("#"))
                {
                    continue;
                }
                if(worksheet.Dimension==null||worksheet.Dimension.End==null) continue;
                ExportSheetI18N(worksheet, list);
            }
        }
        static void ExportSheetI18N(ExcelWorksheet worksheet, Dictionary<string, I18NConfigCategory> list)
        {
            int idIndex = 3;
            int keyIndex = 4;
            for (int row = 6; row <= worksheet.Dimension.End.Row; ++row)
            {
                string prefix = worksheet.Cells[row, 2].Text.Trim();
                if (prefix.Contains("#"))
                {
                    continue;
                }

                if (worksheet.Cells[row, 3].Text.Trim() == "")
                {
                    continue;
                }
                int id = int.Parse(worksheet.Cells[row, idIndex].Text.Trim());
                string key = worksheet.Cells[row, keyIndex].Text.Trim();
                for (int col = keyIndex+1; col <= worksheet.Dimension.End.Column; ++col)
                {
                    I18NConfig config = new I18NConfig();
                    config.Id = id;
                    config.Key = key;
                    config.Value = worksheet.Cells[row, col].Text.Trim();
                    string fieldName = worksheet.Cells[4, col].Text.Trim();
                    if (!list.ContainsKey(fieldName))
                    {
                        list.Add(fieldName,new I18NConfigCategory());
                    }
                    list[fieldName].GetAllList().Add(config);
                }
            }
        }
        
        // 根据生成的类，把json转成protobuf
        private static void ExportI18NExcelProtobuf(Dictionary<string, I18NConfigCategory> list, string relativeDir)
        {
            string dir = GetProtoDir(ConfigType.c, relativeDir);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("namespace TaoTie");
            sb.AppendLine("{");
            sb.AppendLine("    public enum LangType");
            sb.AppendLine("    {");
            int index = 0;
            foreach (var item in list)
            {
                string path = Path.Combine(dir, $"{item.Key}.bytes");
                var bytes = Serializer.Serialize(item.Value);
                using FileStream file = File.Create(path);
                file.Write(bytes);
                sb.AppendLine($"        {item.Key} = {index},");
                index++;
            }
            sb.AppendLine("    }");
            sb.AppendLine("}");
            File.WriteAllText(ClassDir, sb.ToString());
        }
    }
}