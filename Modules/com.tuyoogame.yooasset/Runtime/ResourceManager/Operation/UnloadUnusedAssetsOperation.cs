using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YooAsset
{
    public sealed class UnloadUnusedAssetsOperation : AsyncOperationBase
    {
        private enum ESteps
        {
            None,
            UnloadUnused,
            Done,
        }

        private readonly ResourceManager _resManager;
        private ESteps _steps = ESteps.None;

        internal UnloadUnusedAssetsOperation(ResourceManager resourceManager)
        {
            _resManager = resourceManager;
        }
        internal override void InternalOnStart()
        {
            _steps = ESteps.UnloadUnused;
        }
        internal override void InternalOnUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.UnloadUnused)
            {
                var removeList = new List<LoadBundleFileOperation>(_resManager.LoaderDic.Count);

                // 注意：优先销毁资源提供者
                foreach (var loader in _resManager.LoaderDic.Values)
                {
                    loader.TryDestroyProviders();
                }

                // 获取销毁列表
                foreach (var loader in _resManager.LoaderDic.Values)
                {
                    if (loader.CanDestroyLoader())
                    {
                        removeList.Add(loader);
                    }
                }

                // 销毁文件加载器
                foreach (var loader in removeList)
                {
                    string bundleName = loader.LoadBundleInfo.Bundle.BundleName;
                    loader.DestroyLoader();
                    _resManager.LoaderDic.Remove(bundleName);
                }

                _steps = ESteps.Done;
                Status = EOperationStatus.Succeed;
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
    }
}