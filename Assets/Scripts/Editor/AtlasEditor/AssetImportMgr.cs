using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace TaoTie
{
    /// <summary>
    /// 资源导入管理类
    /// </summary>
    public class AssetImportMgr : AssetPostprocessor
    {

        private static string pattern = "[\u4e00-\u9fbb]";

        static void OnPostprocessAllAssets(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            for (int i = 0; i < importedAssets.Length; i++)
            {
                if (importedAssets[i].Contains("Assets"))
                {
                    if (importedAssets[i].EndsWith(".dds"))
                    {
                        Debug.LogError("纹理不支持.dds,请用其它格式:" + importedAssets[i]);
                        if (EditorUtility.DisplayDialog("提示", "纹理不支持.dds,请转为png、tga等其它格式，或者删除" + importedAssets[i],
                                "忍痛删除"))
                        {
                            File.Delete(importedAssets[i]);
                        }
                    }

                    if (importedAssets[i].Contains(" "))
                    {
                        // Debug.LogError("路径不能包含空格：" + importedAssets[i]);
                    }


                    if (Regex.IsMatch(importedAssets[i], pattern))
                    {
                        Debug.LogError("路径不能包含中文：" + importedAssets[i]);
                    }
                }

            }
        }

        public void OnPostprocessModel(GameObject o)
        {

            if (!assetPath.Contains("Assets"))
            {
                return;
            }

            ModelImporter mi = (ModelImporter) assetImporter;
            if (mi == null)
                return;
            string path = assetPath.ToLower();
            bool oldReadable = mi.isReadable;
            bool newReadable = path.Contains("ReadEnable");

            mi.isReadable = newReadable;
            if (oldReadable != newReadable)
            {
                mi.SaveAndReimport();
            }
        }

        public void OnPreprocessTexture()
        {

            if (!assetPath.Contains("Assets/"))
            {
                return;
            }

            TextureImporter ti = (TextureImporter) assetImporter;
            if (ti == null)
                return;

            //Assets/AssetsPackage 除了UI资源外，其余的纹理都要求是2的幂次方 UI资源是因为会打图集，散图的话主要是背景 先忽略吧
            if (!assetPath.Contains("Assets/AssetsPackage/UI") && assetPath.Contains("Assets/AssetsPackage"))
            {
                var width = 0;
                var height = 0;
                ImportUtil.GetTextureRealWidthAndHeight(ti, ref width, ref height);
                if (!ImportUtil.WidthAndHeightIsPowerOfTwo(width, height) || width != height)
                {
                    Debug.LogError("检测到纹理尺寸不为宽高相同的2的幂次方  路径 = " + assetPath);
                }
            }

            if (assetPath.Contains("Scene/"))
            {
                //场景里面的资源先不用处理，因为会有各种光照贴图，光照探针，烘焙出来的相关的资源，容易出问题
                return;
            }

            if (ti.textureType == TextureImporterType.NormalMap)
            {
                return;
            }

            bool saveAndReimport = false;
            if (ti.mipmapEnabled)
            {
                ti.mipmapEnabled = false;
                saveAndReimport = true;
            }

            if (ti.isReadable)
            {
                ti.isReadable = false;
                saveAndReimport = true;
            }

            if (assetPath.Contains("Assets/AssetsPackage/UI") && ti.textureType != TextureImporterType.Sprite)
            {
                //动态图集需要Texture
                if (assetPath.Contains("DynamicAtlas"))
                {
                    ti.textureType = TextureImporterType.Default;
                }
                else
                {
                    ti.textureType = TextureImporterType.Sprite;
                }

                saveAndReimport = true;
            }

            if (assetPath.Contains("Assets/AssetsPackage/Effect") &&
                ti.textureCompression != TextureImporterCompression.Compressed)
            {
                ti.textureCompression = TextureImporterCompression.Compressed;
                saveAndReimport = true;
            }

            if (saveAndReimport)
            {
                ti.SaveAndReimport();
            }
        }

    }
}
