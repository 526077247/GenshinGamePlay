using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.U2D;
using UnityEngine.TestTools;
using NUnit.Framework;
using YooAsset;

public class TestLoadPanel
{
    public IEnumerator RuntimeTester()
    {
        ResourcePackage package = YooAssets.GetPackage(AssetBundleCollectorDefine.TestPackageName);
        Assert.IsNotNull(package);

        // 异步加载面板
        {
            var assetsHandle = package.LoadAssetAsync<GameObject>("panel_canvas");
            var handleTask = assetsHandle.Task;
            while (!handleTask.IsCompleted)
                yield return null;
            yield return null;
            Assert.AreEqual(EOperationStatus.Succeed, assetsHandle.Status);

            var instantiateOp = assetsHandle.InstantiateAsync();
            var operationTask = instantiateOp.Task;
            while (!handleTask.IsCompleted)
                yield return null;
            yield return null;
            Assert.AreEqual(EOperationStatus.Succeed, instantiateOp.Status);

            Assert.IsNotNull(instantiateOp.Result);
            TestLogger.Log(this, instantiateOp.Result.name);
        }
    }
}