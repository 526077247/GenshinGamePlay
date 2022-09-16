using System;

namespace YooAsset
{
    public class BundleDecryption : IDecryptionServices
    {
        public ulong GetFileOffset(DecryptionFileInfo fileInfo)
        {
            return YooAssetConst.Offset;
        }
    }
}