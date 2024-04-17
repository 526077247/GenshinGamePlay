using YooAsset;
namespace TaoTie
{
    public class PackageInfo
    {
        /// <summary>
        /// 包名
        /// </summary>
        public string Name;
        /// <summary>
        /// 当前版本
        /// </summary>
        public int Ver;
        /// <summary>
        /// 最大版本
        /// </summary>
        public int MaxVer;
        /// <summary>
        /// 状态
        /// </summary>
        public PackageState State;
        /// <summary>
        /// 需要下载大小
        /// </summary>
        public long NeedDownloadSize;
        
        public ResourceDownloaderOperation DownloaderOperation;
    }
}