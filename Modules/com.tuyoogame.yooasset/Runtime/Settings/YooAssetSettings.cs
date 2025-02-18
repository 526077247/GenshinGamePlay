using UnityEngine;

namespace YooAsset
{
    [CreateAssetMenu(fileName = "YooAssetSettings", menuName = "YooAsset/Create YooAsset Settings")]
    public class YooAssetSettings : ScriptableObject
    {
        /// <summary>
        /// YooAsset文件夹名称
        /// </summary>
        public string DefaultYooFolderName = "yoo";

        /// <summary>
        /// 资源清单前缀名称（默认为空)
        /// </summary>
        public string PackageManifestPrefix = string.Empty;
        

        /// <summary>
        /// 清单文件头标记
        /// </summary>
        public const uint ManifestFileSign = 0x594F4F;

        /// <summary>
        /// 清单文件极限大小（100MB）
        /// </summary>
        public const int ManifestFileMaxSize = 104857600;

        /// <summary>
        /// 清单文件格式版本
        /// </summary>
        public const string ManifestFileVersion = "2.2.5";


        /// <summary>
        /// 构建输出文件夹名称
        /// </summary>
        public const string OutputFolderName = "OutputCache";
    }
}