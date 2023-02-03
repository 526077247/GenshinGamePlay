using System;
using System.Threading;
using CommandLine;
namespace TaoTie
{
    internal static class Program
    {
        private static int Main(string[] args)
        {

            try
            {
                MongoRegister.Init();

                // 命令行参数
                Options options = null;
                Parser.Default.ParseArguments<Options>(args)
                        .WithNotParsed(error => throw new Exception($"命令行格式错误!"))
                        .WithParsed(o => { options = o; });

                Options.Instance = options;

                switch (options.AppType)
                {
                    case AppType.ExcelExporter:
                        {
                            ExcelExporter.Export();
                            return 0;
                        }
                    case AppType.CHExcelExporter:
                        {
                            ExcelExporter.Export(true);
                            return 0;
                        }
                    case AppType.AttrExporter:
                        {
                            AttrExporter.Export();
                            return 0;
                        }
                    case AppType.ExporterAll:
                        {
                            ExcelExporter.Export();
                            AttrExporter.Export();
                            return 0;
                        }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            return 1;
        }
    }
}