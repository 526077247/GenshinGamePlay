
namespace YooAsset
{
    internal class RawBundleLoadAssetOperation : FSLoadAssetOperation
    {
        internal override void InternalOnStart()
        {
            Error = $"{nameof(RawBundleLoadAssetOperation)} not support load asset !";
            Status = EOperationStatus.Failed;
        }
        internal override void InternalOnUpdate()
        {
        }
    }
}