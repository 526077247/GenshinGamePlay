
namespace YooAsset
{
    internal sealed class AssetProvider : ProviderOperation
    {
        private FSLoadAssetOperation _loadAssetOp;

        public AssetProvider(ResourceManager manager, string providerGUID, AssetInfo assetInfo) : base(manager, providerGUID, assetInfo)
        {
        }
        protected override void ProcessBundleResult()
        {
            if (_loadAssetOp == null)
            {
                _loadAssetOp = BundleResultObject.LoadAssetAsync(MainAssetInfo);
            }

            if (IsWaitForAsyncComplete)
                _loadAssetOp.WaitForAsyncComplete();

            Progress = _loadAssetOp.Progress;
            if (_loadAssetOp.IsDone == false)
                return;

            if (_loadAssetOp.Status != EOperationStatus.Succeed)
            {
                InvokeCompletion(_loadAssetOp.Error, EOperationStatus.Failed);
            }
            else
            {
                AssetObject = _loadAssetOp.Result;
                InvokeCompletion(string.Empty, EOperationStatus.Succeed);
            }
        }
    }
}