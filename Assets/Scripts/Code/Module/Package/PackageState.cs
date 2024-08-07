namespace TaoTie
{
    public enum PackageState
    {
        /// <summary>
        /// 错误
        /// </summary>
        Error = -1,
        /// <summary>
        /// 未初始化
        /// </summary>
        UnInitialized = 0,
        /// <summary>
        /// 重下Manifest
        /// </summary>
        UpdateManifest,
        /// <summary>
        /// 未下载
        /// </summary>
        UnDownload,
        /// <summary>
        /// 需要更新
        /// </summary>
        NeedUpdate,
        /// <summary>
        /// 正在下载
        /// </summary>
        Downloading,
        /// <summary>
        /// 初始化完成
        /// </summary>
        Initialized,
    }
}