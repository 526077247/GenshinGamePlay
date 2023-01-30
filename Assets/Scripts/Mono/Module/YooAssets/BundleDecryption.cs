using System;

namespace YooAsset
{
    public class BundleDecryption : IDecryptionServices
    {

        /// <summary>
        /// 文件偏移解密方法
        /// </summary>
        public ulong LoadFromFileOffset(DecryptFileInfo fileInfo)
        {
            return 0;
        }

        /// <summary>
        /// 文件内存解密方法
        /// </summary>
        public byte[] LoadFromMemory(DecryptFileInfo fileInfo)
        {
            return null;
        }

        /// <summary>
        /// 文件流解密方法
        /// </summary>
        public System.IO.FileStream LoadFromStream(DecryptFileInfo fileInfo)
        {
            return null;
        }

        /// <summary>
        /// 文件流解密的托管缓存大小
        /// </summary>
        public uint GetManagedReadBufferSize()
        {
            return 0;
        }
    }
}