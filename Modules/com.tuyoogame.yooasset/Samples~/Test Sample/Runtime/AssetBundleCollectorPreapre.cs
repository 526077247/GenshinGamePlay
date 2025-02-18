using System;
using System.IO;
using System.Text;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.U2D;
using UnityEngine.TestTools;
using NUnit.Framework;
using YooAsset;

public class AssetBundleCollectorPreapre : IPrebuildSetup, IPostBuildCleanup
{
    void IPrebuildSetup.Setup()
    {
        AssetBundleCollectorMaker.MakeCollectorSettingData();
    }
    void IPostBuildCleanup.Cleanup()
    {
    }

    [Test]
    public void InitializeYooAssets()
    {
        // 初始化YooAsset
        YooAssets.Initialize();
    }
}