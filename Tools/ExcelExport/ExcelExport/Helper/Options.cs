using CommandLine;

namespace TaoTie
{
    public enum AppType : byte
    {
        ExcelExporter,
        CHExcelExporter,//策划导表校验
        AttrExporter,//导属性
        I18NExporter,//导多语言
        ExporterAll,
    }

    internal class Options
    {
        public static Options Instance { get; set; }

        [Option("AppType", Required = false, Default = AppType.ExporterAll, HelpText = "AppType enum")]
        public AppType AppType { get; set; }

    }
}
