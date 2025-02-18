using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System;

namespace YooAsset
{
    internal abstract class ProviderOperation : AsyncOperationBase
    {
        protected enum ESteps
        {
            None = 0,
            LoadBundleFile,
            ProcessBundleResult,
            Done,
        }

        /// <summary>
        /// 资源提供者唯一标识符
        /// </summary>
        public string ProviderGUID { private set; get; }

        /// <summary>
        /// 资源信息
        /// </summary>
        public AssetInfo MainAssetInfo { private set; get; }

        /// <summary>
        /// 获取的资源对象
        /// </summary>
        public UnityEngine.Object AssetObject { protected set; get; }

        /// <summary>
        /// 获取的资源对象集合
        /// </summary>
        public UnityEngine.Object[] AllAssetObjects { protected set; get; }

        /// <summary>
        /// 获取的资源对象集合
        /// </summary>
        public UnityEngine.Object[] SubAssetObjects { protected set; get; }

        /// <summary>
        /// 获取的场景对象
        /// </summary>
        public UnityEngine.SceneManagement.Scene SceneObject { protected set; get; }

        /// <summary>
        /// 获取的资源包对象
        /// </summary>
        public BundleResult BundleResultObject { protected set; get; }

        /// <summary>
        /// 加载的场景名称
        /// </summary>
        public string SceneName { protected set; get; }

        /// <summary>
        /// 引用计数
        /// </summary>
        public int RefCount { private set; get; } = 0;

        /// <summary>
        /// 是否已经销毁
        /// </summary>
        public bool IsDestroyed { private set; get; } = false;


        private ESteps _steps = ESteps.None;
        private readonly LoadBundleFileOperation _mainBundleLoader;
        private readonly List<LoadBundleFileOperation> _bundleLoaders = new List<LoadBundleFileOperation>();
        private readonly List<HandleBase> _handles = new List<HandleBase>();


        public ProviderOperation(ResourceManager manager, string providerGUID, AssetInfo assetInfo)
        {
            ProviderGUID = providerGUID;
            MainAssetInfo = assetInfo;

            if (string.IsNullOrEmpty(providerGUID) == false)
            {
                // 主资源包加载器
                _mainBundleLoader = manager.CreateMainBundleFileLoader(assetInfo);
                _mainBundleLoader.AddProvider(this);
                _bundleLoaders.Add(_mainBundleLoader);

                // 依赖资源包加载器集合
                var dependLoaders = manager.CreateDependBundleFileLoaders(assetInfo);
                if (dependLoaders.Count > 0)
                    _bundleLoaders.AddRange(dependLoaders);

                // 增加引用计数
                foreach (var bundleLoader in _bundleLoaders)
                {
                    bundleLoader.Reference();
                }
            }
        }
        internal override void InternalOnStart()
        {
            DebugBeginRecording();
            _steps = ESteps.LoadBundleFile;
        }
        internal override void InternalOnUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.LoadBundleFile)
            {
                if (IsWaitForAsyncComplete)
                {
                    foreach (var bundleLoader in _bundleLoaders)
                    {
                        bundleLoader.WaitForAsyncComplete();
                    }
                }

                foreach (var bundleLoader in _bundleLoaders)
                {
                    if (bundleLoader.IsDone == false)
                        return;

                    if (bundleLoader.Status != EOperationStatus.Succeed)
                    {
                        InvokeCompletion(bundleLoader.Error, EOperationStatus.Failed);
                        return;
                    }
                }

                BundleResultObject = _mainBundleLoader.Result;
                if (BundleResultObject == null)
                {
                    string error = $"Loaded bundle result is null !";
                    InvokeCompletion(error, EOperationStatus.Failed);
                    return;
                }

                _steps = ESteps.ProcessBundleResult;
            }

            if (_steps == ESteps.ProcessBundleResult)
            {
                ProcessBundleResult();
            }
        }
        internal override void InternalWaitForAsyncComplete()
        {
            while (true)
            {
                if (ExecuteWhileDone())
                {
                    _steps = ESteps.Done;
                    break;
                }
            }
        }
        protected abstract void ProcessBundleResult();

        /// <summary>
        /// 销毁资源提供者
        /// </summary>
        public void DestroyProvider()
        {
            IsDestroyed = true;

            // 检测是否为正常销毁
            if (IsDone == false)
            {
                Error = "User abort !";
                Status = EOperationStatus.Failed;
            }

            // 减少引用计数
            foreach (var bundleLoader in _bundleLoaders)
            {
                bundleLoader.Release();
            }
        }

        /// <summary>
        /// 是否可以销毁
        /// </summary>
        public bool CanDestroyProvider()
        {
            // 注意：在进行资源加载过程时不可以销毁
            if (_steps == ESteps.ProcessBundleResult)
                return false;

            return RefCount <= 0;
        }

        /// <summary>
        /// 创建资源句柄
        /// </summary>
        public T CreateHandle<T>() where T : HandleBase
        {
            // 引用计数增加
            RefCount++;

            HandleBase handle;
            if (typeof(T) == typeof(AssetHandle))
                handle = new AssetHandle(this);
            else if (typeof(T) == typeof(SceneHandle))
                handle = new SceneHandle(this);
            else if (typeof(T) == typeof(SubAssetsHandle))
                handle = new SubAssetsHandle(this);
            else if (typeof(T) == typeof(AllAssetsHandle))
                handle = new AllAssetsHandle(this);
            else if (typeof(T) == typeof(RawFileHandle))
                handle = new RawFileHandle(this);
            else
                throw new System.NotImplementedException();

            _handles.Add(handle);
            return handle as T;
        }

        /// <summary>
        /// 释放资源句柄
        /// </summary>
        public void ReleaseHandle(HandleBase handle)
        {
            if (RefCount <= 0)
                throw new System.Exception("Should never get here !");

            if (_handles.Remove(handle) == false)
                throw new System.Exception("Should never get here !");

            // 引用计数减少
            RefCount--;
        }

        /// <summary>
        /// 释放所有资源句柄
        /// </summary>
        public void ReleaseAllHandles()
        {
            for (int i = _handles.Count - 1; i >= 0; i--)
            {
                var handle = _handles[i];
                handle.Release();
            }
        }

        /// <summary>
        /// 结束流程
        /// </summary>
        protected void InvokeCompletion(string error, EOperationStatus status)
        {
            DebugEndRecording();

            _steps = ESteps.Done;
            Error = error;
            Status = status;

            // 注意：创建临时列表是为了防止外部逻辑在回调函数内创建或者释放资源句柄。
            // 注意：回调方法如果发生异常，会阻断列表里的后续回调方法！
            List<HandleBase> tempers = new List<HandleBase>(_handles);
            foreach (var hande in tempers)
            {
                if (hande.IsValid)
                {
                    hande.InvokeCallback();
                }
            }
        }

        /// <summary>
        /// 获取下载报告
        /// </summary>
        public DownloadStatus GetDownloadStatus()
        {
            DownloadStatus status = new DownloadStatus();
            foreach (var bundleLoader in _bundleLoaders)
            {
                status.TotalBytes += bundleLoader.LoadBundleInfo.Bundle.FileSize;
                status.DownloadedBytes += bundleLoader.DownloadedBytes;
            }

            if (status.TotalBytes == 0)
                throw new System.Exception("Should never get here !");

            status.IsDone = status.DownloadedBytes == status.TotalBytes;
            status.Progress = (float)status.DownloadedBytes / status.TotalBytes;
            return status;
        }

        #region 调试信息相关
        /// <summary>
        /// 出生的场景
        /// </summary>
        public string SpawnScene = string.Empty;

        /// <summary>
        /// 出生的时间
        /// </summary>
        public string SpawnTime = string.Empty;

        /// <summary>
        /// 加载耗时（单位：毫秒）
        /// </summary>
        public long LoadingTime { protected set; get; }

        // 加载耗时统计
        private Stopwatch _watch = null;

        [Conditional("DEBUG")]
        public void InitSpawnDebugInfo()
        {
            SpawnScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name; ;
            SpawnTime = SpawnTimeToString(UnityEngine.Time.realtimeSinceStartup);
        }
        private string SpawnTimeToString(float spawnTime)
        {
            float h = UnityEngine.Mathf.FloorToInt(spawnTime / 3600f);
            float m = UnityEngine.Mathf.FloorToInt(spawnTime / 60f - h * 60f);
            float s = UnityEngine.Mathf.FloorToInt(spawnTime - m * 60f - h * 3600f);
            return h.ToString("00") + ":" + m.ToString("00") + ":" + s.ToString("00");
        }

        [Conditional("DEBUG")]
        protected void DebugBeginRecording()
        {
            if (_watch == null)
            {
                _watch = Stopwatch.StartNew();
            }
        }

        [Conditional("DEBUG")]
        private void DebugEndRecording()
        {
            if (_watch != null)
            {
                LoadingTime = _watch.ElapsedMilliseconds;
                _watch = null;
            }
        }

        /// <summary>
        /// 获取资源包的调试信息列表
        /// </summary>
        internal void GetBundleDebugInfos(List<DebugBundleInfo> output)
        {
            foreach (var bundleLoader in _bundleLoaders)
            {
                var bundleInfo = new DebugBundleInfo();
                bundleInfo.BundleName = bundleLoader.LoadBundleInfo.Bundle.BundleName;
                bundleInfo.RefCount = bundleLoader.RefCount;
                bundleInfo.Status = bundleLoader.Status;
                output.Add(bundleInfo);
            }
        }
        #endregion
    }
}