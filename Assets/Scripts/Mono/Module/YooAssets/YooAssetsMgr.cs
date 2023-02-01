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

        public BuildInConfig Config;
        public AssetsPackage DefaultPackage;
        public EPlayMode PlayMode;

        private readonly Dictionary<string, AssetsPackage> packages = new Dictionary<string, AssetsPackage>();

        public async ETTask Init(YooAsset.EPlayMode mode)
        {
            PlayMode = mode;
            // 初始化资源系统
            YooAssets.Initialize();
            // 创建默认的资源包
            var package = YooAssets.CreateAssetsPackage("DefaultPackage");
            DefaultPackage = package;
            // 设置该资源包为默认的资源包，可以使用YooAssets相关加载接口加载该资源包内容。
            YooAssets.SetDefaultAssetsPackage(package);
#if UNITY_EDITOR
            // 编辑器下的模拟模式
            if (mode == YooAsset.EPlayMode.EditorSimulateMode)
            {
                var initParameters = new EditorSimulateModeParameters();
                initParameters.SimulatePatchManifestPath = EditorSimulateModeHelper.SimulateBuild("DefaultPackage");
                await package.InitializeAsync(initParameters).Task;
            }
            else
#endif 
            // 单机运行模式
            if (mode == YooAsset.EPlayMode.OfflinePlayMode)
            {
                var initParameters = new OfflinePlayModeParameters();
                await package.InitializeAsync(initParameters).Task;
            }
            // 联机运行模式
            else
            {
                var initParameters = new HostPlayModeParameters();
                initParameters.DefaultHostServer = "";
                initParameters.FallbackHostServer = "";
                await package.InitializeAsync(initParameters).Task;
            }

            await UpdateConfig();
        }

        public async ETTask UpdateConfig()
        {
            var op = DefaultPackage.LoadRawFileSync("config.bytes");
            await op.Task;
            var conf = op.GetRawFileText();
            Config = JsonHelper.FromJson<BuildInConfig>(conf);
            op.Release();
        }

        public void UnloadUnusedAssets()
        {
            DefaultPackage.UnloadUnusedAssets();
        }

        public UpdatePackageManifestOperation UpdateManifestAsync(string version,int timeout)
        {
            return DefaultPackage.UpdatePackageManifestAsync(version, timeout);
        }
    }
}