
namespace YooAsset
{
    internal class DCFSInitializeOperation : FSInitializeFileSystemOperation
    {
        private enum ESteps
        {
            None,
            CheckAppFootPrint,
            SearchCacheFiles,
            VerifyCacheFiles,
            Done,
        }

        private readonly DefaultCacheFileSystem _fileSystem;
        private SearchCacheFilesOperation _searchCacheFilesOp;
        private VerifyCacheFilesOperation _verifyCacheFilesOp;
        private ESteps _steps = ESteps.None;


        internal DCFSInitializeOperation(DefaultCacheFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }
        internal override void InternalOnStart()
        {
#if UNITY_WEBGL
            _steps = ESteps.Done;
            Status = EOperationStatus.Failed;
            Error = $"{nameof(DefaultCacheFileSystem)} is not support WEBGL platform !";
#else
            _steps = ESteps.CheckAppFootPrint;
#endif
        }
        internal override void InternalOnUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.CheckAppFootPrint)
            {
                var appFootPrint = new ApplicationFootPrint(_fileSystem);
                appFootPrint.Load(_fileSystem.PackageName);

                // 如果水印发生变化，则说明覆盖安装后首次打开游戏
                if (appFootPrint.IsDirty())
                {
                    _fileSystem.DeleteAllManifestFiles();
                    appFootPrint.Coverage(_fileSystem.PackageName);
                    YooLogger.Warning("Delete manifest files when application foot print dirty !");
                }

                _steps = ESteps.SearchCacheFiles;
            }

            if (_steps == ESteps.SearchCacheFiles)
            {
                if (_searchCacheFilesOp == null)
                {
                    _searchCacheFilesOp = new SearchCacheFilesOperation(_fileSystem);
                    OperationSystem.StartOperation(_fileSystem.PackageName, _searchCacheFilesOp);
                }

                Progress = _searchCacheFilesOp.Progress;
                if (_searchCacheFilesOp.IsDone == false)
                    return;

                _steps = ESteps.VerifyCacheFiles;
            }

            if (_steps == ESteps.VerifyCacheFiles)
            {
                if (_verifyCacheFilesOp == null)
                {
                    _verifyCacheFilesOp = new VerifyCacheFilesOperation(_fileSystem, _searchCacheFilesOp.Result);
                    OperationSystem.StartOperation(_fileSystem.PackageName, _verifyCacheFilesOp);
                }

                Progress = _verifyCacheFilesOp.Progress;
                if (_verifyCacheFilesOp.IsDone == false)
                    return;

                if (_verifyCacheFilesOp.Status == EOperationStatus.Succeed)
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Succeed;
                    YooLogger.Log($"Package '{_fileSystem.PackageName}' cached files count : {_fileSystem.FileCount}");
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = _verifyCacheFilesOp.Error;
                }
            }
        }
    }
}