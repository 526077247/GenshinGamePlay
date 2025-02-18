using System;
using System.IO;
using System.Text;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using YooAsset;

public static class AssetBundleCollectorMaker
{
    public static void MakeCollectorSettingData()
    {
#if UNITY_EDITOR
        // 清空旧数据
        YooAsset.Editor.AssetBundleCollectorSettingData.ClearAll();

        // 创建正常文件Package
        var testPackage = YooAsset.Editor.AssetBundleCollectorSettingData.CreatePackage(AssetBundleCollectorDefine.TestPackageName);
        testPackage.EnableAddressable = true;
        testPackage.AutoCollectShaders = true;
        testPackage.IgnoreRuleName = "NormalIgnoreRule";

        // 音频
        var audioGroup = YooAsset.Editor.AssetBundleCollectorSettingData.CreateGroup(testPackage, "AudioGroup");
        {
            var collector1 = new YooAsset.Editor.AssetBundleCollector();
            collector1.CollectPath = "";
            collector1.CollectorGUID = "bbce3e09a17b55c46b5615e995b5fc70"; //TestRes/Audios目录
            collector1.CollectorType = YooAsset.Editor.ECollectorType.MainAssetCollector;
            collector1.PackRuleName = nameof(YooAsset.Editor.PackSeparately);
            YooAsset.Editor.AssetBundleCollectorSettingData.CreateCollector(audioGroup, collector1);
        }

        // 图片
        var imageGroup = YooAsset.Editor.AssetBundleCollectorSettingData.CreateGroup(testPackage, "ImageGroup");
        {
            var collector1 = new YooAsset.Editor.AssetBundleCollector();
            collector1.CollectPath = "";
            collector1.CollectorGUID = "d4768b7c3d3101d4ea693f95b337861d"; //TestRes/Image目录
            collector1.CollectorType = YooAsset.Editor.ECollectorType.MainAssetCollector;
            collector1.PackRuleName = nameof(YooAsset.Editor.PackSeparately);
            YooAsset.Editor.AssetBundleCollectorSettingData.CreateCollector(imageGroup, collector1);
        }

        // 图集
        var spriteGroup = YooAsset.Editor.AssetBundleCollectorSettingData.CreateGroup(testPackage, "SpriteGroup");
        {
            var collector1 = new YooAsset.Editor.AssetBundleCollector();
            collector1.CollectPath = "";
            collector1.CollectorGUID = "634f8145b892c554ba440c212b36a933"; //TestRes/SpriteAtlas目录
            collector1.CollectorType = YooAsset.Editor.ECollectorType.MainAssetCollector;
            collector1.PackRuleName = nameof(YooAsset.Editor.PackSeparately);
            YooAsset.Editor.AssetBundleCollectorSettingData.CreateCollector(spriteGroup, collector1);

            var collector2 = new YooAsset.Editor.AssetBundleCollector();
            collector2.CollectPath = "";
            collector2.CollectorGUID = "e41a9b5f04378154f9bd69ac5d52ec44"; //TestRes/Sprites目录
            collector2.CollectorType = YooAsset.Editor.ECollectorType.StaticAssetCollector;
            collector2.PackRuleName = nameof(YooAsset.Editor.PackDirectory);
            YooAsset.Editor.AssetBundleCollectorSettingData.CreateCollector(spriteGroup, collector2);
        }

        // 面板
        var panelGroup = YooAsset.Editor.AssetBundleCollectorSettingData.CreateGroup(testPackage, "PanelGroup");
        {
            var collector1 = new YooAsset.Editor.AssetBundleCollector();
            collector1.CollectPath = "";
            collector1.CollectorGUID = "4e9a00d6e825d644b9be75155d88daa6"; //TestRes/Panel目录
            collector1.CollectorType = YooAsset.Editor.ECollectorType.MainAssetCollector;
            collector1.PackRuleName = nameof(YooAsset.Editor.PackSeparately);
            YooAsset.Editor.AssetBundleCollectorSettingData.CreateCollector(panelGroup, collector1);
        }

        // 预制体
        var prefabGroup = YooAsset.Editor.AssetBundleCollectorSettingData.CreateGroup(testPackage, "PrefabGroup");
        {
            var collector1 = new YooAsset.Editor.AssetBundleCollector();
            collector1.CollectPath = "";
            collector1.CollectorGUID = "8da7a00d90270b44898e9b165f86f005"; //TestRes/Prefab目录
            collector1.CollectorType = YooAsset.Editor.ECollectorType.MainAssetCollector;
            collector1.PackRuleName = nameof(YooAsset.Editor.PackDirectory);
            YooAsset.Editor.AssetBundleCollectorSettingData.CreateCollector(prefabGroup, collector1);
        }

        // 场景
        var sceneGroup = YooAsset.Editor.AssetBundleCollectorSettingData.CreateGroup(testPackage, "SceneGroup");
        {
            var collector1 = new YooAsset.Editor.AssetBundleCollector();
            collector1.CollectPath = "";
            collector1.CollectorGUID = "3e169b07999abb0489113f5f4c015c89"; //TestRes/Scene目录
            collector1.CollectorType = YooAsset.Editor.ECollectorType.MainAssetCollector;
            collector1.PackRuleName = nameof(YooAsset.Editor.PackSeparately);
            YooAsset.Editor.AssetBundleCollectorSettingData.CreateCollector(sceneGroup, collector1);
        }

        // 序列化文件
        var scriptableObjectGroup = YooAsset.Editor.AssetBundleCollectorSettingData.CreateGroup(testPackage, "ScriptableObjectGroup");
        {
            var collector1 = new YooAsset.Editor.AssetBundleCollector();
            collector1.CollectPath = "";
            collector1.CollectorGUID = "af885cf1a7abb8c44bd9d139409d2961"; //TestRes/ScriptableObject目录
            collector1.CollectorType = YooAsset.Editor.ECollectorType.MainAssetCollector;
            collector1.PackRuleName = nameof(YooAsset.Editor.PackSeparately);
            YooAsset.Editor.AssetBundleCollectorSettingData.CreateCollector(scriptableObjectGroup, collector1);
        }

        // 创建原生文件Package
        var rawPackage = YooAsset.Editor.AssetBundleCollectorSettingData.CreatePackage(AssetBundleCollectorDefine.RawPackageName);
        rawPackage.EnableAddressable = true;
        rawPackage.AutoCollectShaders = true;
        rawPackage.IgnoreRuleName = "RawFileIgnoreRule";

        // 原生文件
        var rawFileGroup = YooAsset.Editor.AssetBundleCollectorSettingData.CreateGroup(rawPackage, "RawFileGroup");
        {
            var collector1 = new YooAsset.Editor.AssetBundleCollector();
            collector1.CollectPath = "";
            collector1.CollectorGUID = "fddaaf9430e24344196cc82ac3d006b4"; //TestRes/RawFiles目录
            collector1.CollectorType = YooAsset.Editor.ECollectorType.MainAssetCollector;
            collector1.PackRuleName = nameof(YooAsset.Editor.PackRawFile);
            YooAsset.Editor.AssetBundleCollectorSettingData.CreateCollector(rawFileGroup, collector1);
        }

        // 视频文件
        var videoFileGroup = YooAsset.Editor.AssetBundleCollectorSettingData.CreateGroup(rawPackage, "VideoFileGroup");
        {
            var collector1 = new YooAsset.Editor.AssetBundleCollector();
            collector1.CollectPath = "";
            collector1.CollectorGUID = "9028a60fd472239448b89453084bfa0a"; //TestRes/Video目录
            collector1.CollectorType = YooAsset.Editor.ECollectorType.MainAssetCollector;
            collector1.PackRuleName = nameof(YooAsset.Editor.PackVideoFile);
            YooAsset.Editor.AssetBundleCollectorSettingData.CreateCollector(videoFileGroup, collector1);
        }

        // 修正配置路径为空导致的错误
        YooAsset.Editor.AssetBundleCollectorSettingData.FixFile();
#endif
    }
}