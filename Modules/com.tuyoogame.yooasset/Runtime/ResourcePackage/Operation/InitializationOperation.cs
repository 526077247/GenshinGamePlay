
namespace YooAsset
{
    /// <summary>
    /// 初始化操作
    /// </summary>
    public abstract class InitializationOperation : AsyncOperationBase
    {
    }

    /// <summary>
    /// 编辑器下模拟模式
    /// </summary>
    internal sealed class EditorSimulateModeInitializationOperation : InitializationOperation
    {
        private enum ESteps
        {
            None,
            CreateFileSystem,
            InitFileSystem,
            Done,
        }

        private readonly EditorSimulateModeImpl _impl;
        private readonly EditorSimulateModeParameters _parameters;
        private FSInitializeFileSystemOperation _initFileSystemOp;
        private ESteps _steps = ESteps.None;

        internal EditorSimulateModeInitializationOperation(EditorSimulateModeImpl impl, EditorSimulateModeParameters parameters)
        {
            _impl = impl;
            _parameters = parameters;
        }
        internal override void InternalOnStart()
        {
            _steps = ESteps.CreateFileSystem;
        }
        internal override void InternalOnUpdate()
        {
            if (_steps == ESteps.CreateFileSystem)
            {
                if (_parameters.EditorFileSystemParameters == null)
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = "Editor file system parameters is null";
                    return;
                }

                _impl.EditorFileSystem = _parameters.EditorFileSystemParameters.CreateFileSystem(_impl.PackageName);
                if (_impl.EditorFileSystem == null)
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = "Failed to create editor file system";
                    return;
                }

                _steps = ESteps.InitFileSystem;
            }

            if (_steps == ESteps.InitFileSystem)
            {
                if (_initFileSystemOp == null)
                    _initFileSystemOp = _impl.EditorFileSystem.InitializeFileSystemAsync();

                Progress = _initFileSystemOp.Progress;
                if (_initFileSystemOp.IsDone == false)
                    return;

                if (_initFileSystemOp.Status == EOperationStatus.Succeed)
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Succeed;
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = _initFileSystemOp.Error;
                }
            }
        }
    }

    /// <summary>
    /// 离线运行模式
    /// </summary>
    internal sealed class OfflinePlayModeInitializationOperation : InitializationOperation
    {
        private enum ESteps
        {
            None,
            CreateFileSystem,
            InitFileSystem,
            Done,
        }

        private readonly OfflinePlayModeImpl _impl;
        private readonly OfflinePlayModeParameters _parameters;
        private FSInitializeFileSystemOperation _initFileSystemOp;
        private FSRequestPackageVersionOperation _requestPackageVersionOp;
        private FSLoadPackageManifestOperation _loadPackageManifestOp;
        private ESteps _steps = ESteps.None;

        internal OfflinePlayModeInitializationOperation(OfflinePlayModeImpl impl, OfflinePlayModeParameters parameters)
        {
            _impl = impl;
            _parameters = parameters;
        }
        internal override void InternalOnStart()
        {
            _steps = ESteps.CreateFileSystem;
        }
        internal override void InternalOnUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.CreateFileSystem)
            {
                if (_parameters.BuildinFileSystemParameters == null)
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = "Buildin file system parameters is null";
                    return;
                }

                _impl.BuildinFileSystem = _parameters.BuildinFileSystemParameters.CreateFileSystem(_impl.PackageName);
                if (_impl.BuildinFileSystem == null)
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = "Failed to create buildin file system";
                    return;
                }

                _steps = ESteps.InitFileSystem;
            }

            if (_steps == ESteps.InitFileSystem)
            {
                if (_initFileSystemOp == null)
                    _initFileSystemOp = _impl.BuildinFileSystem.InitializeFileSystemAsync();

                Progress = _initFileSystemOp.Progress;
                if (_initFileSystemOp.IsDone == false)
                    return;

                if (_initFileSystemOp.Status == EOperationStatus.Succeed)
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Succeed;
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = _initFileSystemOp.Error;
                }
            }
        }
    }

    /// <summary>
    /// 联机运行模式
    /// </summary>
    internal sealed class HostPlayModeInitializationOperation : InitializationOperation
    {
        private enum ESteps
        {
            None,
            CreateBuildinFileSystem,
            InitBuildinFileSystem,
            CreateCacheFileSystem,
            InitCacheFileSystem,
            Done,
        }

        private readonly HostPlayModeImpl _impl;
        private readonly HostPlayModeParameters _parameters;
        private FSInitializeFileSystemOperation _initBuildinFileSystemOp;
        private FSInitializeFileSystemOperation _initCacheFileSystemOp;
        private ESteps _steps = ESteps.None;

        internal HostPlayModeInitializationOperation(HostPlayModeImpl impl, HostPlayModeParameters parameters)
        {
            _impl = impl;
            _parameters = parameters;
        }
        internal override void InternalOnStart()
        {
            _steps = ESteps.CreateBuildinFileSystem;
        }
        internal override void InternalOnUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.CreateBuildinFileSystem)
            {
                if (_parameters.BuildinFileSystemParameters == null)
                {
                    _steps = ESteps.CreateCacheFileSystem;
                    return;
                }

                _impl.BuildinFileSystem = _parameters.BuildinFileSystemParameters.CreateFileSystem(_impl.PackageName);
                if (_impl.BuildinFileSystem == null)
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = "Failed to create buildin file system";
                    return;
                }

                _steps = ESteps.InitBuildinFileSystem;
            }

            if (_steps == ESteps.InitBuildinFileSystem)
            {
                if (_initBuildinFileSystemOp == null)
                    _initBuildinFileSystemOp = _impl.BuildinFileSystem.InitializeFileSystemAsync();

                Progress = _initBuildinFileSystemOp.Progress;
                if (_initBuildinFileSystemOp.IsDone == false)
                    return;

                if (_initBuildinFileSystemOp.Status == EOperationStatus.Succeed)
                {
                    _steps = ESteps.CreateCacheFileSystem;
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = _initBuildinFileSystemOp.Error;
                }
            }

            if (_steps == ESteps.CreateCacheFileSystem)
            {
                if (_parameters.CacheFileSystemParameters == null)
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = "Cache file system parameters is null";
                    return;
                }

                _impl.CacheFileSystem = _parameters.CacheFileSystemParameters.CreateFileSystem(_impl.PackageName);
                if (_impl.CacheFileSystem == null)
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = "Failed to create cache file system";
                    return;
                }

                _steps = ESteps.InitCacheFileSystem;
            }

            if (_steps == ESteps.InitCacheFileSystem)
            {
                if (_initCacheFileSystemOp == null)
                    _initCacheFileSystemOp = _impl.CacheFileSystem.InitializeFileSystemAsync();

                Progress = _initCacheFileSystemOp.Progress;
                if (_initCacheFileSystemOp.IsDone == false)
                    return;

                if (_initCacheFileSystemOp.Status == EOperationStatus.Succeed)
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Succeed;
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = _initCacheFileSystemOp.Error;
                }
            }
        }
    }

    /// <summary>
    /// WebGL运行模式
    /// </summary>
    internal sealed class WebPlayModeInitializationOperation : InitializationOperation
    {
        private enum ESteps
        {
            None,
            CreateWebServerFileSystem,
            InitWebServerFileSystem,
            CreateWebRemoteFileSystem,
            InitWebRemoteFileSystem,
            CheckResult,
            Done,
        }

        private readonly WebPlayModeImpl _impl;
        private readonly WebPlayModeParameters _parameters;
        private FSInitializeFileSystemOperation _initWebServerFileSystemOp;
        private FSInitializeFileSystemOperation _initWebRemoteFileSystemOp;
        private ESteps _steps = ESteps.None;

        internal WebPlayModeInitializationOperation(WebPlayModeImpl impl, WebPlayModeParameters parameters)
        {
            _impl = impl;
            _parameters = parameters;
        }
        internal override void InternalOnStart()
        {
            _steps = ESteps.CreateWebServerFileSystem;
        }
        internal override void InternalOnUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.CreateWebServerFileSystem)
            {
                if (_parameters.WebServerFileSystemParameters == null)
                {
                    _steps = ESteps.CreateWebRemoteFileSystem;
                    return;
                }

                _impl.WebServerFileSystem = _parameters.WebServerFileSystemParameters.CreateFileSystem(_impl.PackageName);
                if (_impl.WebServerFileSystem == null)
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = "Failed to create web server file system";
                    return;
                }

                _steps = ESteps.InitWebServerFileSystem;
            }

            if (_steps == ESteps.InitWebServerFileSystem)
            {
                if (_initWebServerFileSystemOp == null)
                    _initWebServerFileSystemOp = _impl.WebServerFileSystem.InitializeFileSystemAsync();

                Progress = _initWebServerFileSystemOp.Progress;
                if (_initWebServerFileSystemOp.IsDone == false)
                    return;

                if (_initWebServerFileSystemOp.Status == EOperationStatus.Succeed)
                {
                    _steps = ESteps.CreateWebRemoteFileSystem;
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = _initWebServerFileSystemOp.Error;
                }
            }

            if (_steps == ESteps.CreateWebRemoteFileSystem)
            {
                if (_parameters.WebRemoteFileSystemParameters == null)
                {
                    _steps = ESteps.CheckResult;
                    return;
                }

                _impl.WebRemoteFileSystem = _parameters.WebRemoteFileSystemParameters.CreateFileSystem(_impl.PackageName);
                if (_impl.WebRemoteFileSystem == null)
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = "Failed to create web remote file system";
                    return;
                }

                _steps = ESteps.InitWebRemoteFileSystem;
            }

            if (_steps == ESteps.InitWebRemoteFileSystem)
            {
                if (_initWebRemoteFileSystemOp == null)
                    _initWebRemoteFileSystemOp = _impl.WebRemoteFileSystem.InitializeFileSystemAsync();

                Progress = _initWebRemoteFileSystemOp.Progress;
                if (_initWebRemoteFileSystemOp.IsDone == false)
                    return;

                if (_initWebRemoteFileSystemOp.Status == EOperationStatus.Succeed)
                {
                    _steps = ESteps.CheckResult;
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = _initWebRemoteFileSystemOp.Error;
                }
            }

            if (_steps == ESteps.CheckResult)
            {
                if (_impl.WebServerFileSystem == null && _impl.WebRemoteFileSystem == null)
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = "Not found any file system !";
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Succeed;
                }
            }
        }
    }
}