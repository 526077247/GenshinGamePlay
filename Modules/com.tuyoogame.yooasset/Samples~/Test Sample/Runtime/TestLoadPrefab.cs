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

public class TestLoadPrefab
{
    public IEnumerator RuntimeTester()
    {
        ResourcePackage package = YooAssets.GetPackage(AssetBundleCollectorDefine.TestPackageName);
        Assert.IsNotNull(package);

        // 异步加载所有预制体
        {
            var allAssetsHandle = package.LoadAllAssetsAsync<GameObject>("prefab_a");
            yield return allAssetsHandle;
            Assert.AreEqual(EOperationStatus.Succeed, allAssetsHandle.Status);

            var allAssetObjects = allAssetsHandle.AllAssetObjects;
            Assert.IsNotNull(allAssetObjects);

            int count = allAssetObjects.Count;
            Assert.AreEqual(count, 3);
        }

        // 异步加载指定预制体
        {
            var assetsHandle = package.LoadAssetAsync<GameObject>("prefab_b");
            yield return assetsHandle;
            Assert.AreEqual(EOperationStatus.Succeed, assetsHandle.Status);
            Assert.IsNotNull(assetsHandle.AssetObject);

            var instantiateOp = assetsHandle.InstantiateAsync();
            yield return instantiateOp;
            Assert.AreEqual(EOperationStatus.Succeed, instantiateOp.Status);

            Assert.IsNotNull(instantiateOp.Result);
            TestLogger.Log(this, instantiateOp.Result.name);
        }

        // 同步加载指定预制体
        {
            var assetsHandle = package.LoadAssetSync<GameObject>("prefab_c");
            yield return assetsHandle;
            Assert.AreEqual(EOperationStatus.Succeed, assetsHandle.Status);
            Assert.IsNotNull(assetsHandle.AssetObject);

            var instantiateOp = assetsHandle.InstantiateAsync();
            yield return instantiateOp;
            Assert.AreEqual(EOperationStatus.Succeed, instantiateOp.Status);

            Assert.IsNotNull(instantiateOp.Result);
            TestLogger.Log(this, instantiateOp.Result.name);
        }
    }
}