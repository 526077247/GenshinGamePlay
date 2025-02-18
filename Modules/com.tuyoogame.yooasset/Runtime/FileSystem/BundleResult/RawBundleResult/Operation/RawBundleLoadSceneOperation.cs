
namespace YooAsset
{
    internal class RawBundleLoadSceneOperation : FSLoadSceneOperation
    {
        internal override void InternalOnStart()
        {
            Error = $"{nameof(RawBundleLoadSceneOperation)} not support load scene !";
            Status = EOperationStatus.Failed;
        }
        internal override void InternalOnUpdate()
        {
        }
        public override void UnSuspendLoad()
        {
        }
    }
}