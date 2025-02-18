
namespace YooAsset
{
    public abstract class ClearCacheFilesOperation : AsyncOperationBase
    {
    }
    internal sealed class ClearCacheFilesImplOperation : ClearCacheFilesOperation
    {
        private enum ESteps
        {
            None,
            ClearFileSystemA,
            ClearFileSystemB,
            ClearFileSystemC,
            Done,
        }

        private readonly IPlayMode _impl;
        private readonly IFileSystem _fileSystemA;
        private readonly IFileSystem _fileSystemB;
        private readonly IFileSystem _fileSystemC;
        private readonly string _clearMode;
        private readonly object _clearParam;
        private FSClearCacheFilesOperation _clearCacheFilesOpA;
        private FSClearCacheFilesOperation _clearCacheFilesOpB;
        private FSClearCacheFilesOperation _clearCacheFilesOpC;
        private ESteps _steps = ESteps.None;
        
        internal ClearCacheFilesImplOperation(IPlayMode impl, IFileSystem fileSystemA, IFileSystem fileSystemB, IFileSystem fileSystemC, string clearMode, object clearParam)
        {
            _impl = impl;
            _fileSystemA = fileSystemA;
            _fileSystemB = fileSystemB;
            _fileSystemC = fileSystemC;
            _clearMode = clearMode;
            _clearParam = clearParam;
        }
        internal override void InternalOnStart()
        {
            _steps = ESteps.ClearFileSystemA;
        }
        internal override void InternalOnUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.ClearFileSystemA)
            {
                if (_fileSystemA == null)
                {
                    _steps = ESteps.ClearFileSystemB;
                    return;
                }

                if (_clearCacheFilesOpA == null)
                    _clearCacheFilesOpA = _fileSystemA.ClearCacheFilesAsync(_impl.ActiveManifest, _clearMode, _clearParam);

                Progress = _clearCacheFilesOpA.Progress;
                if (_clearCacheFilesOpA.IsDone == false)
                    return;

                if (_clearCacheFilesOpA.Status == EOperationStatus.Succeed)
                {
                    _steps = ESteps.ClearFileSystemB;
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = _clearCacheFilesOpA.Error;
                }
            }

            if (_steps == ESteps.ClearFileSystemB)
            {
                if (_fileSystemB == null)
                {
                    _steps = ESteps.ClearFileSystemC;
                    return;
                }

                if (_clearCacheFilesOpB == null)
                    _clearCacheFilesOpB = _fileSystemB.ClearCacheFilesAsync(_impl.ActiveManifest, _clearMode, _clearParam);

                Progress = _clearCacheFilesOpB.Progress;
                if (_clearCacheFilesOpB.IsDone == false)
                    return;

                if (_clearCacheFilesOpB.Status == EOperationStatus.Succeed)
                {
                    _steps = ESteps.ClearFileSystemC;
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = _clearCacheFilesOpB.Error;
                }
            }

            if (_steps == ESteps.ClearFileSystemC)
            {
                if (_fileSystemC == null)
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Succeed;
                    return;
                }

                if (_clearCacheFilesOpC == null)
                    _clearCacheFilesOpC = _fileSystemC.ClearCacheFilesAsync(_impl.ActiveManifest, _clearMode, _clearParam);

                Progress = _clearCacheFilesOpC.Progress;
                if (_clearCacheFilesOpC.IsDone == false)
                    return;

                if (_clearCacheFilesOpC.Status == EOperationStatus.Succeed)
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Succeed;
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = _clearCacheFilesOpC.Error;
                }
            }
        }
    }
}