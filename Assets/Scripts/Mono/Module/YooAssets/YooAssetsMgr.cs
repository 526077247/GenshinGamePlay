using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TaoTie;
using UnityEditor;
using UnityEngine;

namespace YooAsset
{
    public class YooAssetsMgr
    {
        public static YooAssetsMgr Instance { get; private set; } = new YooAssetsMgr();

        public BuildInConfig Config { get; private set; }
        
        public static string StaticVersionStreamingPath = Path.Combine(Application.streamingAssetsPath,"YooAssets", YooAssetSettings.VersionFileName);
        
        public static string PatchManifestStreamingPath = Path.Combine(Application.streamingAssetsPath,"YooAssets", "PatchManifest_{0}.bytes");
        public static string PatchManifestPersistentPath = PathHelper.MakePersistentLoadPath("PatchManifest_{0}.bytes");

        private PatchManifest buildInManifest;
        private PatchManifest staticManifest;

        public int staticVersion;
        public bool IsDllBuildIn;
        public IEnumerator Init(YooAssets.EPlayMode mode)
        {
            IsDllBuildIn = false;
            var _downloader1 = new UnityWebDataRequester();
            _downloader1.SendRequest(StaticVersionStreamingPath);
            while (!_downloader1.IsDone())
            {
                yield return 0;
            }
            int.TryParse(_downloader1.GetText(),out int buildInVersion);
            _downloader1.Dispose();
            staticVersion = PlayerPrefs.GetInt("STATIC_VERSION", -1);
#if !UNITY_EDITOR
            this.IsDllBuildIn = staticVersion == buildInVersion;//粗略判断一下
#endif
            if (staticVersion == -1)
            {
                staticVersion = buildInVersion;
                PlayerPrefs.SetInt("STATIC_VERSION", staticVersion);
            }
            Debug.Log("buildInVersion"+buildInVersion+" staticVersion"+staticVersion);
            string path = string.Format(PatchManifestStreamingPath, buildInVersion);
            _downloader1 = new UnityWebDataRequester();
            _downloader1.SendRequest(path);
            while (!_downloader1.IsDone())
            {
                yield return 0;
            }
            var jStr = _downloader1.GetText();
            _downloader1.Dispose();
            Debug.Log("Load buildInManifest at"+path+" jstr == null?"+string.IsNullOrEmpty(jStr));
            if(!string.IsNullOrEmpty(jStr))
                buildInManifest= Deserialize(jStr);
            if (staticVersion > buildInVersion)
            {
                path = string.Format(PatchManifestPersistentPath, staticVersion);
                _downloader1 = new UnityWebDataRequester();
                _downloader1.SendRequest(path);
                while (!_downloader1.IsDone())
                {
                    yield return 0;
                }
                jStr = _downloader1.GetText();
                _downloader1.Dispose();
                Debug.Log("Load buildInManifest at"+path+" jstr == null?"+string.IsNullOrEmpty(jStr));
                if(!string.IsNullOrEmpty(jStr))
                    staticManifest = Deserialize(jStr);
            }

            if (mode == YooAssets.EPlayMode.EditorSimulateMode)
            {
                string jstr = File.ReadAllText("Assets/AssetsPackage/config.bytes");
                Config = JsonHelper.FromJson<BuildInConfig>(jstr);
            }
            else
            {
                var ab = SyncLoadAssetBundle("assets/assetspackage.bundle");

                string jstr = ((TextAsset) ab.LoadAsset("Assets/AssetsPackage/config.bytes", typeof (TextAsset))).text;
                Config = JsonHelper.FromJson<BuildInConfig>(jstr);
                ab.Unload(true);
            }
        }


        #region 之所以是有这些接口，是为了在启动时进行使用，加快启动速度，其他地方严禁调用这里的方法
        /// <summary>
        /// 反序列化
        /// </summary>
        public PatchManifest Deserialize(string jsonData)
        {
            PatchManifest patchManifest = JsonUtility.FromJson<PatchManifest>(jsonData);
            
            // BundleList
            foreach (var patchBundle in patchManifest.BundleList)
            {
                patchBundle.ParseFlagsValue();
                patchBundle.ParseFileName(patchManifest.OutputNameStyle);
                patchManifest.BundleDic.Add(patchBundle.BundleName, patchBundle);
            }

            // AssetList
            foreach (var patchAsset in patchManifest.AssetList)
            {
                // 注意：我们不允许原始路径存在重名
                string assetPath = patchAsset.AssetPath;
                if (patchManifest.AssetDic.ContainsKey(assetPath))
                    throw new Exception($"AssetPath have existed : {assetPath}");
                else
                    patchManifest.AssetDic.Add(assetPath, patchAsset);
            }

            return patchManifest;
        }
        
        /// <summary>
        /// 获取assetbundle在本地的存储路径，该接口只会检查本地数据
        /// </summary>
        /// <param name="assetBundleName"></param>
        /// <returns></returns>
        public string GetAssetBundleLocalPath(string assetBundleName)
        {
            PatchBundle info;
            if (IsAssetBundleInPackage(assetBundleName))
            {
                info = buildInManifest.BundleDic[assetBundleName];
                return PathHelper.MakeStreamingLoadPath(info.FileName);
            }
            if(staticManifest.BundleDic.TryGetValue(assetBundleName,out info))
                return PathHelper.MakePersistentLoadPath("CacheFiles/"+info.FileName);
            throw new Exception("指定assetBundleName不存在！"+assetBundleName);
        }

        /// <summary>
        /// 判断assetbundle是否是内置在包里面的
        /// </summary>
        /// <param name="assetBundleName"></param>
        /// <returns></returns>
        public bool IsAssetBundleInPackage(string assetBundleName)
        {
            if (buildInManifest.BundleDic.TryGetValue(assetBundleName,out var info))
            {
                if (staticManifest==null||!staticManifest.BundleDic.TryGetValue(assetBundleName, out var nowInfo) || nowInfo.Hash == info.Hash)
                {
                    return true;
                }
                Debug.Log(assetBundleName+" staticManifest==null"+(staticManifest==null));
                Debug.Log(assetBundleName+" nowInfo.Hash == info.Hash"+(nowInfo.Hash == info.Hash));
            }
            Debug.Log(assetBundleName+" !buildInManifest.BundleDic.Contains");
            return false;
        }

        /// <summary>
        /// 同步加载ab包
        /// </summary>
        /// <param name="assetBundleName"></param>
        /// <returns></returns>
        public AssetBundle SyncLoadAssetBundle(string assetBundleName)
        {
            var path = GetAssetBundleLocalPath(assetBundleName);
            var ab = AssetBundle.LoadFromFile(path, 0, YooAssetConst.Offset);
            return ab;
        }
        
        
        private AssetBundle configBundle;
        
        public TextAsset LoadTextAsset(string addressPath)
        {
            addressPath = "Assets/AssetsPackage/" + addressPath;
            if (YooAssets.PlayMode != YooAssets.EPlayMode.EditorSimulateMode)
            {
                if (configBundle == null)
                {
                    configBundle = SyncLoadAssetBundle("assets/assetspackage/config.bundle");
                }
                TextAsset asset = (TextAsset)configBundle.LoadAsset(addressPath, typeof(TextAsset));
                if (asset == null)
                {
                    Debug.LogError("LoadTextAsset fail, path: "+ addressPath);
                }
                return asset;
                
            }
#if UNITY_EDITOR
            else
            {
                TextAsset asset = (AssetDatabase.LoadAssetAtPath(addressPath, typeof(TextAsset)) as TextAsset);
                if (asset == null)
                {
                    Debug.LogError("LoadTextAsset fail, path: " + addressPath);
                }
                return asset;
            }
#endif
            return null;
        }
        
        public Dictionary<string, TextAsset> LoadAllTextAsset()
        {
            Dictionary<string, TextAsset> res = new Dictionary<string, TextAsset>();
            if (YooAssets.PlayMode != YooAssets.EPlayMode.EditorSimulateMode)
            {
                if (configBundle == null)
                {
                    configBundle = SyncLoadAssetBundle("assets/assetspackage/config.bundle");
                }
                var assets = configBundle.LoadAllAssets<TextAsset>();
                foreach (TextAsset asset in assets)
                {
                    res.Add(asset.name, asset);
                }
            }
#if UNITY_EDITOR
            else
            {
                var fullPath = "Assets/AssetsPackage/Config/";
                if (Directory.Exists(fullPath))
                {
                    DirectoryInfo direction = new DirectoryInfo(fullPath);
                    FileInfo[] files = direction.GetFiles("*", SearchOption.AllDirectories);
                    for (int i = 0; i < files.Length; i++)
                    {
                        if (files[i].Name.EndsWith(".meta"))
                        {
                            continue;
                        }

                        var asset = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/AssetsPackage/Config/" + files[i].Name);
                        res.Add(asset.name, asset);
                    }
                }
            }
#endif
            return res;
        }

        /// <summary>
        /// 清除配置ab包
        /// </summary>
        public void ClearConfigCache()
        {
            configBundle?.Unload(true);
            configBundle = null;
        }
        
        #endregion
    }
}