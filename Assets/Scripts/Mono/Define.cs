﻿
namespace TaoTie
{
    public static class Define
    { 
        public const byte KEY = 64;
        public const string BuildOutputDir = "./Temp/Bin/Debug";
        public const string DefaultName = "DefaultPackage";
        public static bool IsSH;
        
        public const string HotfixLoadDir = "Code/Hotfix/";
#if UNITY_ANDROID
        public const string AOTLoadDir = "Code/AOTAndroid/";
#elif UNITY_IOS
        public const string AOTLoadDir = "Code/AOTiOS/";
#else //不同BuildTarget的裁剪AOT dll一般不复用。
        public const string AOTLoadDir = "Code/AOT/";
#endif
        public const string HotfixDir = "Assets/AssetsPackage/" + HotfixLoadDir;
        public const string AOTDir = "Assets/AssetsPackage/" + AOTLoadDir;
#if UNITY_EDITOR
        public static readonly bool Debug = true;
#else
        public static readonly bool Debug = UnityEngine.Debug.isDebugBuild;
#endif
        public static readonly int DesignScreenWidth = 1920;
        public static readonly int DesignScreenHeight = 1080;
        public static int LogLevel = 1;

        
#if FORCE_UPDATE // 是否默认强更 该配置项会影响到有无网络对游戏更新流程的改变
	    public static bool ForceUpdate = true; //默认强更，无网络或更新失败会只能选退出游戏或重试
#else
        public static bool ForceUpdate = false; //默认不强更，无网络或更新失败可以选跳过或重试
#endif
            public static bool Networked =
#if UNITY_EDITOR
                    false;
#else
        UnityEngine.Application.internetReachability != UnityEngine.NetworkReachability.NotReachable;
#endif

        public static int Process = 1;

        /// <summary>
        /// 0:Json 1:Bytes
        /// </summary>
        public static int ConfigType = 
#if RoslynAnalyzer
                1;
#else
                0;
#endif
        
        public static readonly string[] RenameList = {"iOS"};
    }
}