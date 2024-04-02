using System;
using System.IO;

namespace YooAsset
{
    public class FileOffsetEncryption: IEncryptionServices
    {
        public EncryptResult Encrypt(EncryptFileInfo fileInfo)
        {
            int offset = 32;
            byte[] fileData = File.ReadAllBytes(fileInfo.FilePath);
            var encryptedData = new byte[fileData.Length + offset];
            Buffer.BlockCopy(fileData, 0, encryptedData, offset, fileData.Length);

            EncryptResult result = new EncryptResult();
            result.LoadMethod = EBundleLoadMethod.LoadFromFileOffset;
            result.EncryptedData = encryptedData;
            return result;
            // {
            //     EncryptResult result = new EncryptResult();
            //     result.LoadMethod = EBundleLoadMethod.Normal;
            //     return result;
            // }
        }
    }
}