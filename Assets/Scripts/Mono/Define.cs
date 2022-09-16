
namespace TaoTie
{
    public static class Define
    {
        public const string BuildOutputDir = "./Temp/Bin/Debug";
        
        public const string HotfixDir = "Assets/AssetsPackage/Code/Hotfix/";
#if UNITY_EDITOR
        public static readonly bool Debug = true;
#else
        public static readonly bool Debug = false;
#endif
        public static readonly int DesignScreen_Width = 1366;
        public static readonly int DesignScreen_Height = 768;
        public const int LogLevel = 1;

        
#if FORCE_UPDATE // 是否默认强更 该配置项会影响到有无网络对游戏更新流程的改变
	public static bool ForceUpdate = true; //默认强更，无网络或更新失败会只能选退出游戏或重试
#else
        public static bool ForceUpdate = false; //默认不强更，无网络或更新失败可以选跳过更新或重试
#endif
        public static bool Networked = false;

        public static int Process = 1;
    }
}