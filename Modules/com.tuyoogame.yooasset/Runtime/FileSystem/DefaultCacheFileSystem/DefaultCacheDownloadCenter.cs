using System;
using System.Collections.Generic;

namespace YooAsset
{
    internal class DefaultCacheDownloadCenter
    {
        private readonly DefaultCacheFileSystem _fileSystem;
        protected readonly Dictionary<string, DefaultDownloadFileOperation> _downloaders = new Dictionary<string, DefaultDownloadFileOperation>(1000);
        protected readonly List<string> _removeDownloadList = new List<string>(1000);

        public DefaultCacheDownloadCenter(DefaultCacheFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        /// <summary>
        /// 更新下载中心
        /// </summary>
        public void Update()
        {
            // 获取可移除的下载器集合
            _removeDownloadList.Clear();
            foreach (var valuePair in _downloaders)
            {
                var downloader = valuePair.Value;

                // 注意：主动终止引用计数为零的下载任务
                if (downloader.RefCount <= 0)
                {
                    _removeDownloadList.Add(valuePair.Key);
                    downloader.SetAbort();
                    continue;
                }

                if (downloader.IsDone)
                {
                    _removeDownloadList.Add(valuePair.Key);
                    continue;
                }
            }

            // 移除下载器
            foreach (var key in _removeDownloadList)
            {
                _downloaders.Remove(key);
            }

            // 最大并发数检测
            int processCount = GetProcessingOperationCount();
            if (processCount != _downloaders.Count)
            {
                if (processCount < _fileSystem.DownloadMaxConcurrency)
                {
                    int startCount = _fileSystem.DownloadMaxConcurrency - processCount;
                    if (startCount > _fileSystem.DownloadMaxRequestPerFrame)
                        startCount = _fileSystem.DownloadMaxRequestPerFrame;

                    foreach (var operationPair in _downloaders)
                    {
                        var operation = operationPair.Value;
                        if (operation.Status == EOperationStatus.None)
                        {
                            OperationSystem.StartOperation(_fileSystem.PackageName, operation);
                            startCount--;
                            if (startCount <= 0)
                                break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 创建下载任务
        /// </summary>
        public FSDownloadFileOperation DownloadFileAsync(PackageBundle bundle, DownloadParam param)
        {
            // 查询旧的下载器
            if (_downloaders.TryGetValue(bundle.BundleGUID, out var oldDownloader))
            {
                oldDownloader.Reference();
                return oldDownloader;
            }

            // 设置请求URL
            if (string.IsNullOrEmpty(param.ImportFilePath))
            {
                param.MainURL = _fileSystem.RemoteServices.GetRemoteMainURL(bundle.FileName);
                param.FallbackURL = _fileSystem.RemoteServices.GetRemoteFallbackURL(bundle.FileName);
            }
            else
            {
                // 注意：把本地文件路径指定为远端下载地址
                param.MainURL = DownloadSystemHelper.ConvertToWWWPath(param.ImportFilePath);
                param.FallbackURL = param.MainURL;
            }

            // 创建新的下载器
            DefaultDownloadFileOperation newDownloader;
            if (bundle.FileSize >= _fileSystem.ResumeDownloadMinimumSize)
            {
                newDownloader = new DownloadResumeFileOperation(_fileSystem, bundle, param);
                newDownloader.Reference();
                _downloaders.Add(bundle.BundleGUID, newDownloader);
            }
            else
            {
                newDownloader = new DownloadNormalFileOperation(_fileSystem, bundle, param);
                newDownloader.Reference();
                _downloaders.Add(bundle.BundleGUID, newDownloader);
            }
            return newDownloader;
        }

        /// <summary>
        /// 获取正在进行中的下载器总数
        /// </summary>
        private int GetProcessingOperationCount()
        {
            int count = 0;
            foreach (var operationPair in _downloaders)
            {
                var operation = operationPair.Value;
                if (operation.Status != EOperationStatus.None)
                    count++;
            }
            return count;
        }
    }
}