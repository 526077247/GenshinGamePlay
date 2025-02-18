#if UNITY_WEBGL && WEIXINMINIGAME
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using YooAsset;
using WeChatWASM;
using static UnityEngine.Networking.UnityWebRequest;

internal class WXFSClearUnusedBundleFilesAsync : FSClearCacheFilesOperation
{
    private enum ESteps
    {
        None,
        GetUnusedCacheFiles,
        WaitingSearch,
        ClearUnusedCacheFiles,
        Done,
    }

    private readonly WechatFileSystem _fileSystem;
    private readonly PackageManifest _manifest;
    private List<string> _unusedCacheFiles;
    private int _unusedFileTotalCount = 0;
    private ESteps _steps = ESteps.None;

    internal WXFSClearUnusedBundleFilesAsync(WechatFileSystem fileSystem, PackageManifest manifest)
    {
        _fileSystem = fileSystem;
        _manifest = manifest;
    }
    internal override void InternalOnStart()
    {
        _steps = ESteps.GetUnusedCacheFiles;
    }
    internal override void InternalOnUpdate()
    {
        if (_steps == ESteps.None || _steps == ESteps.Done)
            return;

        if (_steps == ESteps.GetUnusedCacheFiles)
        {
            _steps = ESteps.WaitingSearch;

            var fileSystemMgr = _fileSystem.GetFileSystemMgr();
            var statOption = new WXStatOption();
            statOption.path = _fileSystem.FileRoot;
            statOption.recursive = true;
            statOption.success = (WXStatResponse response) =>
            {
                foreach (var fileStat in response.stats)
                {
                    // 注意：存储文件必须按照Bundle文件哈希值存储！
                    string bundleGUID = Path.GetFileNameWithoutExtension(fileStat.path);
                    if (_manifest.TryGetPackageBundleByBundleGUID(bundleGUID, out PackageBundle value) == false)
                    {
                        string fullPath = WX.GetCachePath(fileStat.path);
                        if (_unusedCacheFiles.Contains(fullPath) == false)
                            _unusedCacheFiles.Add(fullPath);
                    }
                }

                _steps = ESteps.ClearUnusedCacheFiles;
                _unusedFileTotalCount = _unusedCacheFiles.Count;
                YooLogger.Log($"Found unused cache files count : {_unusedFileTotalCount}");
            };
            statOption.fail = (WXStatResponse response) =>
            {
                _steps = ESteps.Done;
                Status = EOperationStatus.Failed;
                Error = response.errMsg;
            };
            fileSystemMgr.Stat(statOption);
        }

        if (_steps == ESteps.ClearUnusedCacheFiles)
        {
            for (int i = _unusedCacheFiles.Count - 1; i >= 0; i--)
            {
                string filePath = _unusedCacheFiles[i];
                _unusedCacheFiles.RemoveAt(i);
                WX.RemoveFile(filePath, null);

                if (OperationSystem.IsBusy)
                    break;
            }

            if (_unusedFileTotalCount == 0)
                Progress = 1.0f;
            else
                Progress = 1.0f - (_unusedCacheFiles.Count / _unusedFileTotalCount);

            if (_unusedCacheFiles.Count == 0)
            {
                _steps = ESteps.Done;
                Status = EOperationStatus.Succeed;
            }
        }
    }
}
#endif