using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.U2D;
using Object = UnityEngine.Object;

namespace TaoTie
{

    public class AtlasHelper
    {
        ///=========================================================================================
        public const string AtlasName = "Atlas";
        public const string TextureName = "Textures";
        public const string DiscreteImagesName = "DiscreteImages";
        public static readonly string[] uipaths = {"UI", "UIGame", /*"UIHall"*/};

        public static readonly Dictionary<string, int> MaxSize = new Dictionary<string, int>()
        {
            {"Standalone",4096},
            {"iPhone", 2048},
            {"Android", 2048},
            {"WebGL", 1024},
        };
        public enum ImageType
        {
            Atlas,
            DiscreteImages,
            Texture
        }
        /// <summary>
        /// 将UI目录下的小图 打成  图集
        /// </summary>
        public static void GeneratingAtlas()
        {
            for (int i = 0; i < uipaths.Length; i++)
            {
                //将UI目录下的Atlas 打成 图集
                string uiPath = Path.Combine(Application.dataPath, "AssetsPackage", uipaths[i]);
                DirectoryInfo uiDirInfo = new DirectoryInfo(uiPath);
                foreach (DirectoryInfo dirInfo in uiDirInfo.GetDirectories())
                {
                    GeneratingAtlasByDir(dirInfo);
                }
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public static void GeneratingAtlasByDir(DirectoryInfo dirInfo)
        {
            //目录是否有Atlas目录
            bool hasAtlas = false;
            //目录是否有DiscreteImages目录
            bool hasDiscreteImages = false;
            //目录是否有Texture目录
            bool hasTextureImages = false;
            foreach (DirectoryInfo seconddirInfo in dirInfo.GetDirectories())
            {
                if (seconddirInfo.Name == AtlasName)
                {
                    hasAtlas = true;
                }

                if (seconddirInfo.Name == DiscreteImagesName)
                {
                    hasDiscreteImages = true;
                }
                
                if (seconddirInfo.Name == TextureName)
                {
                    hasTextureImages = true;
                }
            }

            if (hasAtlas)
            {
                //Atlas目录下是否还有目录
                DirectoryInfo atlasDirInfo = new DirectoryInfo(Path.Combine(dirInfo.FullName, AtlasName));

                SetImagesFormat(atlasDirInfo, ImageType.Atlas);

                foreach (DirectoryInfo atlasDir in atlasDirInfo.GetDirectories())
                {
                    CreateAtlasByFolders(dirInfo, atlasDir);
                }

                //Atlas目录上的小图打成一个图集
                CreateAtlasBySprite(dirInfo);
            }

            if (hasDiscreteImages)
            {
                //DiscreteImages目录下的所有图片
                DirectoryInfo discreteImagesDirInfo =
                    new DirectoryInfo(Path.Combine(dirInfo.FullName, DiscreteImagesName));
                SetImagesFormat(discreteImagesDirInfo, ImageType.DiscreteImages);
            }

            if (hasTextureImages)
            {
                //DiscreteImages目录下的所有图片
                DirectoryInfo discreteImagesDirInfo =
                    new DirectoryInfo(Path.Combine(dirInfo.FullName, TextureName));
                SetImagesFormat(discreteImagesDirInfo, ImageType.Texture);
            }
            
        }

        /// <summary>
        /// 设置图片压缩格式
        /// </summary>
        private static void SetImagesFormat(DirectoryInfo discreteImagesDirInfo, ImageType type = ImageType.Atlas)
        {
            foreach (FileInfo pngFile in discreteImagesDirInfo.GetFiles("*.*", SearchOption.AllDirectories))
            {
                if (pngFile.Extension.Equals(".meta"))
                {
                    continue;
                }

                string allPath = pngFile.FullName;
                string assetPath = allPath.Substring(allPath.IndexOf("Assets"));
                Object sprite = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
                if (IsPackable(sprite))
                {
                    //Logger.LogError("=============" + assetPath);
                    var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;

                    if (type == ImageType.Texture)
                    {
                        if(importer.textureType == TextureImporterType.Sprite) importer.textureType = TextureImporterType.Default;
                    }
                    else
                    {
                        importer.textureType = TextureImporterType.Sprite;
                        importer.spriteImportMode = SpriteImportMode.Single;
                        importer.spritePixelsPerUnit = 100;
                        
                        TextureImporterSettings textureImportSetting = new TextureImporterSettings();
                        importer.ReadTextureSettings(textureImportSetting);
                        textureImportSetting.spriteMeshType = SpriteMeshType.FullRect;
                        textureImportSetting.spriteExtrude = 1;
                        textureImportSetting.spriteGenerateFallbackPhysicsShape = false;
                        importer.SetTextureSettings(textureImportSetting);
                    }
                    
                    importer.mipmapEnabled = false;
                    importer.isReadable = false;
                    importer.wrapMode = TextureWrapMode.Clamp;
                    importer.filterMode = FilterMode.Bilinear;
                    importer.alphaIsTransparency = true;
                    importer.alphaSource = TextureImporterAlphaSource.FromInput;
                    importer.sRGBTexture = true;
                    //TextureImporterCompression type = TextureImporterCompression.Compressed;
                    TextureImporterFormat format = TextureImporterFormat.ASTC_6x6;
                    if (assetPath.Contains("Uncompressed") || type == ImageType.Atlas)
                    {
                        format = TextureImporterFormat.RGBA32;
                    }

                    TextureImporterPlatformSettings platformSetting = importer.GetPlatformTextureSettings("iPhone");
                    platformSetting.maxTextureSize = MaxSize["iPhone"];
                    platformSetting.resizeAlgorithm = TextureResizeAlgorithm.Mitchell;
                    platformSetting.overridden = true;
                    //.compressionQuality = 100;
                    //platformSetting.textureCompression = type;
                    platformSetting.format = format;
                    importer.SetPlatformTextureSettings(platformSetting);

                    platformSetting = importer.GetPlatformTextureSettings("Android");
                    platformSetting.maxTextureSize =  MaxSize["Android"];
                    platformSetting.resizeAlgorithm = TextureResizeAlgorithm.Mitchell;
                    platformSetting.overridden = true;
                    // platformSetting.textureCompression = type;
                    platformSetting.format = format;
                    importer.SetPlatformTextureSettings(platformSetting);
                    
                    platformSetting = importer.GetPlatformTextureSettings("WebGL");
                    platformSetting.maxTextureSize = MaxSize["WebGL"];
                    platformSetting.resizeAlgorithm = TextureResizeAlgorithm.Mitchell;
                    platformSetting.overridden = true;
                    // platformSetting.textureCompression = type;
#if UNITY_2021_3_OR_NEWER
                    platformSetting.format = format;
#else
                    platformSetting.format = type == ImageType.Atlas?format:TextureImporterFormat.DXT5;
#endif
                    importer.SetPlatformTextureSettings(platformSetting);

                    platformSetting = importer.GetPlatformTextureSettings("Standalone");
                    platformSetting.maxTextureSize = MaxSize["Standalone"];
                    platformSetting.resizeAlgorithm = TextureResizeAlgorithm.Mitchell;
                    platformSetting.overridden = true;
                    // platformSetting.textureCompression = type;
                    platformSetting.format = type == ImageType.Atlas?format:TextureImporterFormat.DXT5;
                    importer.SetPlatformTextureSettings(platformSetting);

                    importer.SaveAndReimport();
                }

            }
        }


        private static void CreateAtlasBySprite(DirectoryInfo dirInfo)
        {
            //add sprite
            List<Sprite> spts = new List<Sprite>();

            DirectoryInfo atlasDirs = new DirectoryInfo(Path.Combine(dirInfo.FullName, AtlasName));

            spts.Clear();
            foreach (FileInfo pngFile in atlasDirs.GetFiles("*.png", SearchOption.TopDirectoryOnly))
            {

                string allPath = pngFile.FullName;
                string assetPath = allPath.Substring(allPath.IndexOf("Assets"));
                Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
                if (IsPackable(sprite))
                {
                    spts.Add(sprite);
                }
            }

            string
                atlasName = AtlasName +
                            ".spriteatlas"; //dirInfo.Name.ToLower() + "_" + AtlasName.ToLower() + ".spriteatlas";
            SpriteAtlas sptAtlas = GetOrCreateAtlas(dirInfo.FullName, atlasName);
            string dirInfoPath = dirInfo.FullName.Substring(dirInfo.FullName.IndexOf("Assets"));
            SetSpriteAtlas(sptAtlas, Path.Combine(dirInfoPath, atlasName));
            if (sptAtlas != null && spts.Count > 0)
            {
                AddPackAtlas(sptAtlas, spts.ToArray());
            }
        }


        //通过文件夹创建atlas
        private static void CreateAtlasByFolders(DirectoryInfo dirInfo, DirectoryInfo atlasDir)
        {
            //add folders
            List<Object> folders = new List<Object>();
            folders.Clear();

            string atlasDirPath = atlasDir.FullName.Substring(dirInfo.FullName.IndexOf("Assets"));
            var o = AssetDatabase.LoadAssetAtPath<DefaultAsset>(atlasDirPath);
            if (IsPackable(o))
            {
                folders.Add(o);
            }


            string atlasName = AtlasName + "_" + atlasDir.Name + ".spriteatlas";
            SpriteAtlas sptAtlas = GetOrCreateAtlas(dirInfo.FullName, atlasName);
            string assetPath = dirInfo.FullName.Substring(dirInfo.FullName.IndexOf("Assets"));
            SetSpriteAtlas(sptAtlas, Path.Combine(assetPath, atlasName));
            if (sptAtlas != null && folders.Count > 0)
            {
                AddPackAtlas(sptAtlas, folders.ToArray());
            }
        }

        /// <summary>
        /// 设置SpriteAtlas 的配置参数  Uncompressed 目录的图集不压
        /// </summary>
        /// <param name="atlas"></param>
        /// <param name="atlasPath"></param>
        /// <param name="readWrite"></param>
        private static void SetSpriteAtlas(SpriteAtlas atlas, string atlasPath, bool readWrite = false)
        {
            TextureImporterFormat format = TextureImporterFormat.ASTC_6x6;
            if (atlasPath.Contains("Uncompressed"))
            {
                format = TextureImporterFormat.RGBA32;
            }

            // 设置参数 可根据项目具体情况进行设置
            SpriteAtlasPackingSettings packSetting = new SpriteAtlasPackingSettings()
            {
                blockOffset = 1,
                enableRotation = false,
                enableTightPacking = false,
                padding = 2,
            };
            atlas.SetPackingSettings(packSetting);

            SpriteAtlasTextureSettings oldSetting = atlas.GetTextureSettings();
            SpriteAtlasTextureSettings textureSetting = new SpriteAtlasTextureSettings()
            {
                readable = oldSetting.readable | readWrite,
                generateMipMaps = false,
                sRGB = true,
                filterMode = FilterMode.Bilinear,
            };
            atlas.SetTextureSettings(textureSetting);

            TextureImporterPlatformSettings platformSetting = new TextureImporterPlatformSettings()
            {
                name = "Android",
                maxTextureSize = MaxSize["Android"],
                format = format,
                overridden = true,
            };

            atlas.SetPlatformSettings(platformSetting);

            platformSetting = new TextureImporterPlatformSettings()
            {
                name = "iPhone",
                maxTextureSize = MaxSize["iPhone"],
                format = format,
                overridden = true,
            };

            atlas.SetPlatformSettings(platformSetting);
            
            platformSetting = new TextureImporterPlatformSettings()
            {
                name = "WebGL",
                maxTextureSize = 2048,
#if UNITY_2021_3_OR_NEWER
                format = format,
#else
                format = TextureImporterFormat.DXT5; //设置格式
#endif
                overridden = true, 
            };

            atlas.SetPlatformSettings(platformSetting);
            platformSetting = new TextureImporterPlatformSettings()
            {
                name = "Standalone",
                maxTextureSize = MaxSize["Standalone"],
                format = TextureImporterFormat.DXT5,
                overridden = true,
            };

            atlas.SetPlatformSettings(platformSetting);
        }



        private static void AddPackAtlas(SpriteAtlas atlas, Object[] spt)
        {
            //MethodInfo methodInfo = System.Type
            //     .GetType("UnityEditor.U2D.SpriteAtlasExtensions, UnityEditor")
            //     .GetMethod("Add", BindingFlags.Public | BindingFlags.Static);
            //if (methodInfo != null)
            //    methodInfo.Invoke(null, new object[] { atlas, spt });
            //else
            //    Debug.Log("methodInfo is null");
            SpriteAtlasExtensions.Add(atlas, spt);
            PackAtlas(atlas);
        }

        private static void PackAtlas(SpriteAtlas atlas)
        {
            //System.Type
            //    .GetType("UnityEditor.U2D.SpriteAtlasUtility, UnityEditor")
            //    .GetMethod("PackAtlases", BindingFlags.NonPublic | BindingFlags.Static)
            //    .Invoke(null, new object[] { new[] { atlas }, EditorUserBuildSettings.activeBuildTarget });

            UnityEditor.U2D.SpriteAtlasUtility.PackAtlases(new[] {atlas}, EditorUserBuildSettings.activeBuildTarget);

            //MethodInfo getPreviewTextureMI = typeof(SpriteAtlasExtensions).GetMethod("GetPreviewTextures", BindingFlags.Static | BindingFlags.NonPublic);
            //Texture2D[] atlasTextures = (Texture2D[])getPreviewTextureMI.Invoke(null, new System.Object[] { atlas });

        }

        private static bool IsPackable(Object o)
        {
            return o != null && (o.GetType() == typeof(Sprite) || o.GetType() == typeof(Texture2D) ||
                                 (o.GetType() == typeof(DefaultAsset) &&
                                  ProjectWindowUtil.IsFolder(o.GetInstanceID())));
        }


        private static SpriteAtlas GetOrCreateAtlas(string fullName, string atlasName)
        {
            string filePath = Path.Combine(fullName, atlasName);
            string atlasPath = filePath.Substring(filePath.IndexOf("Assets"));
            SpriteAtlas sa = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(atlasPath);
            if (sa == null)
            {
                sa = new SpriteAtlas();
                AssetDatabase.CreateAsset(sa, atlasPath);
                EditorUtility.SetDirty(sa);
                AssetDatabase.SaveAssetIfDirty(sa);
            }
            else
            {
                var obj = sa.GetPackables();
                sa.Remove(obj);
                EditorUtility.SetDirty(sa);
                AssetDatabase.SaveAssetIfDirty(sa);
            }
            return sa;
            //        string yaml = @"%YAML 1.1
            //%TAG !u! tag:unity3d.com,2011:
            //--- !u!687078895 &4343727234628468602
            //SpriteAtlas:
            //  m_ObjectHideFlags: 0
            //  m_PrefabParentObject: {fileID: 0}
            //  m_PrefabInternal: {fileID: 0}
            //  m_Name: New Sprite Atlas
            //  m_EditorData:
            //    textureSettings:
            //      serializedVersion: 2
            //      anisoLevel: 1
            //      compressionQuality: 50
            //      maxTextureSize: 2048
            //      textureCompression: 0
            //      filterMode: 1
            //      generateMipMaps: 0
            //      readable: 0
            //      crunchedCompression: 0
            //      sRGB: 1
            //    platformSettings: []
            //    packingParameters:
            //      serializedVersion: 2
            //      padding: 4
            //      blockOffset: 1
            //      allowAlphaSplitting: 0
            //      enableRotation: 1
            //      enableTightPacking: 1
            //    variantMultiplier: 1
            //    packables: []
            //    bindAsDefault: 1
            //  m_MasterAtlas: {fileID: 0}
            //  m_PackedSprites: []
            //  m_PackedSpriteNamesToIndex: []
            //  m_Tag: New Sprite Atlas
            //  m_IsVariant: 0
            //";
            //        AssetDatabase.Refresh();

            //        string filePath = Path.Combine(fullName, atlasName);
            //        if (File.Exists(filePath))
            //        {
            //            File.Delete(filePath);
            //            AssetDatabase.Refresh();
            //        }
            //        FileStream fs = new FileStream(filePath, FileMode.CreateNew);
            //        byte[] bytes = new UTF8Encoding().GetBytes(yaml);
            //        fs.Write(bytes, 0, bytes.Length);
            //        fs.Close();
            //        AssetDatabase.Refresh();
        }

        /*
         * 清理图集资源
         */
        public static void ClearAllAtlas()
        {
            foreach (var cpath in uipaths)
            {
                string uiPath = Path.Combine(Application.dataPath, "AssetsPackage", cpath);
                string[] strs = FileHelper.GetFileNames(uiPath, "*.spriteatlas*", true);
                for (int i = 0; i < strs.Length; i++)
                {
                    string atlasPath = strs[i].Substring(strs[i].IndexOf("Assets"));
                    Debug.Log(atlasPath);
                    AssetDatabase.DeleteAsset(atlasPath);
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }


        public static void SettingPNG()
        {
            /*    string[] fileStrs = Directory.GetFiles(Path.GetFullPath("Assets/AssetsPackage/UI"), "*.*", SearchOption.AllDirectories);
                foreach(var file in fileStrs)
                {
                    Debug.Log("file:" + file);
                    if(file.EndsWith(".meta"))
                    {
                        continue;
                    }
                    var textureImporter = AssetImporter.GetAtPath( FindReferences.GetRelativeAssetsPath(file) ) as TextureImporter;
                    if(textureImporter == null)
                    {
                        continue;
                    }
                    TextureImporterPlatformSettings settingAndroid = textureImporter.GetPlatformTextureSettings("Android");
                    settingAndroid.overridden = true;
                    settingAndroid.format = TextureImporterFormat.ETC2_RGBA8;  //设置格式
                    textureImporter.SetPlatformTextureSettings(settingAndroid);
    
                    textureImporter.SaveAndReimport();
                }
                */
            string[] paths = Directory.GetFileSystemEntries(Path.GetFullPath("Assets/AssetsPackage"));
            for (int i = 0; i < paths.Length; i++)
            {
                if (Directory.Exists(paths[i]))
                {
                    DirectoryInfo di = new DirectoryInfo(paths[i]);

                    if (di.Name == "Tmp" || di.Name == "Fonts" || di.Name == "FmodBanks" ||
                        di.Name == "Shaders")
                    {
                        continue;
                    }

                    bool hasUI = false;
                    for (int j = 0; j < uipaths.Length; j++)
                    {
                        if (di.Name == uipaths[j])
                        {
                            hasUI = true;
                            break;
                        }
                    }

                    if (hasUI)
                    {
                        continue;
                    }

                    string[] fileStrs = Directory.GetFiles(Path.GetFullPath("Assets/AssetsPackage/" + di.Name), "*.*",
                        SearchOption.AllDirectories);
                    foreach (var file in fileStrs)
                    {
                        if (file.EndsWith(".meta"))
                        {
                            continue;
                        }

                        var textureImporter =
                            AssetImporter.GetAtPath(FileHelper.GetRelativeAssetsPath(file)) as TextureImporter;
                        if (textureImporter == null)
                        {
                            continue;
                        }
                        
                        TextureImporterPlatformSettings setting = textureImporter.GetPlatformTextureSettings("Android");
                        setting.overridden = true;
                        setting.format = TextureImporterFormat.ASTC_6x6; //设置格式
                        setting.maxTextureSize = MaxSize["Android"];
                        textureImporter.SetPlatformTextureSettings(setting);

                        setting = textureImporter.GetPlatformTextureSettings("iPhone");
                        setting.overridden = true;
                        setting.format = TextureImporterFormat.ASTC_6x6; //设置格式
                        setting.maxTextureSize = MaxSize["iPhone"];
                        textureImporter.SetPlatformTextureSettings(setting);

                        textureImporter.SaveAndReimport();
                        
                        setting = textureImporter.GetPlatformTextureSettings("WebGL");
                        setting.overridden = true;
#if UNITY_2021_3_OR_NEWER
                        setting.format = TextureImporterFormat.ASTC_6x6; //设置格式
                        setting.maxTextureSize = MaxSize["WebGL"];
#else
                        setting.format = TextureImporterFormat.DXT5; //设置格式
                        setting.maxTextureSize = GetTextureDXT5Size(textureImporter, file, MaxSize["WebGL"]);
#endif
                        textureImporter.SetPlatformTextureSettings(setting);

                        textureImporter.SaveAndReimport();
                        
                        setting = textureImporter.GetPlatformTextureSettings("Standalone");
                        setting.overridden = true;
                        setting.format = TextureImporterFormat.DXT5; //设置格式
                        setting.maxTextureSize = GetTextureDXT5Size(textureImporter, file, MaxSize["Standalone"]);
                        textureImporter.SetPlatformTextureSettings(setting);

                        textureImporter.SaveAndReimport();
                    }


                }
            }
        }

        private static int GetTextureDXT5Size(TextureImporter textureImporter,string path,int maxSize)
        {
            int width = 0; int height = 0; 
            ImportUtil.GetTextureRealWidthAndHeight(textureImporter, ref width, ref height);
            if (Get2Flag(width) && Get2Flag(height))
            {
                var max = Math.Max(width, height);
                return Math.Min(max, maxSize);
            }
            if (Math.Max(width, height) % Math.Min(width, height) != 0)
            {
                if(textureImporter.textureType == TextureImporterType.Sprite)
                    Debug.LogError("不为2的幂"+path);
            }
            var res = Math.Max(width, height);
            res++;
            res |= res >> 1;
            res |= res >> 2;
            res |= res >> 4;
            res |= res >> 8;
            res |= res >> 16;
            res = (res < 0) ? 1 : res + 1;
            return Math.Min(res >> 1, maxSize);
        }

        private static bool Get2Flag(int num)
        {
            if (num < 1) return false;
            return (num & num - 1) == 0;
        }
    }
}

