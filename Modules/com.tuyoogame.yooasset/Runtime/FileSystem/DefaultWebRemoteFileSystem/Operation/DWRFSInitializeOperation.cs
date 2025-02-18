
namespace YooAsset
{
    internal class DWRFSInitializeOperation : FSInitializeFileSystemOperation
    {
        private readonly DefaultWebRemoteFileSystem _fileSystem;

        public DWRFSInitializeOperation(DefaultWebRemoteFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }
        internal override void InternalOnStart()
        {
            Status = EOperationStatus.Succeed;
        }
        internal override void InternalOnUpdate()
        {
        }
    }
}