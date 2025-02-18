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

public class EditorFileSystemTester : IPrebuildSetup, IPostBuildCleanup
{
    private const string EFS_TEST_PACKAGE_ROOT_KEY = "EFS_TEST_PACKAGE_ROOT_KEY";
    private const string EFS_RAW_PACKAGE_ROOT_KEY = "EFS_RAW_PACKAGE_ROOT_KEY";

    void IPrebuildSetup.Setup()
    {
#if UNITY_EDITOR
        // 构建TestPackage
        {
            var simulateParams = new PackageInvokeBuildParam(AssetBundleCollectorDefine.TestPackageName);
            simulateParams.BuildPipelineName = "EditorSimulateBuildPipeline";
            simulateParams.InvokeAssmeblyName = "YooAsset.Test.Editor";
            simulateParams.InvokeClassFullName = "TestPackageBuilder";
            simulateParams.InvokeMethodName = "BuildPackage";
            var simulateResult = PakcageInvokeBuilder.InvokeBuilder(simulateParams);
            UnityEditor.EditorPrefs.SetString(EFS_TEST_PACKAGE_ROOT_KEY, simulateResult.PackageRootDirectory);
        }

        // 构建RawPackage
        {
            var simulateParams = new PackageInvokeBuildParam(AssetBundleCollectorDefine.RawPackageName);
            simulateParams.BuildPipelineName = "EditorSimulateBuildPipeline";
            simulateParams.InvokeAssmeblyName = "YooAsset.Test.Editor";
            simulateParams.InvokeClassFullName = "TestPackageBuilder";
            simulateParams.InvokeMethodName = "BuildPackage";
            var simulateResult = PakcageInvokeBuilder.InvokeBuilder(simulateParams);
            UnityEditor.EditorPrefs.SetString(EFS_RAW_PACKAGE_ROOT_KEY, simulateResult.PackageRootDirectory);
        }
#endif
    }
    void IPostBuildCleanup.Cleanup()
    {
    }

    [UnityTest]
    public IEnumerator DestroyPackage()
    {
        // 销毁旧资源包
        {
            var package = YooAssets.GetPackage(AssetBundleCollectorDefine.TestPackageName);
            var destroyOp = package.DestroyAsync();
            yield return destroyOp;
            if (destroyOp.Status != EOperationStatus.Succeed)
                Debug.LogError(destroyOp.Error);
            Assert.AreEqual(EOperationStatus.Succeed, destroyOp.Status);

            bool result = YooAssets.RemovePackage(AssetBundleCollectorDefine.TestPackageName);
            Assert.IsTrue(result);
        }

        // 销毁旧资源包
        {
            var package = YooAssets.GetPackage(AssetBundleCollectorDefine.RawPackageName);
            var destroyOp = package.DestroyAsync();
            yield return destroyOp;
            if (destroyOp.Status != EOperationStatus.Succeed)
                Debug.LogError(destroyOp.Error);
            Assert.AreEqual(EOperationStatus.Succeed, destroyOp.Status);

            bool result = YooAssets.RemovePackage(AssetBundleCollectorDefine.RawPackageName);
            Assert.IsTrue(result);
        }
    }

    [UnityTest]
    public IEnumerator InitializePackage()
    {
        // 初始化TestPackage
        {
            string packageRoot = string.Empty;
#if UNITY_EDITOR
            packageRoot = UnityEditor.EditorPrefs.GetString(EFS_TEST_PACKAGE_ROOT_KEY);
#endif
            if (Directory.Exists(packageRoot) == false)
                throw new Exception($"Not found package root : {packageRoot}");

            var package = YooAssets.CreatePackage(AssetBundleCollectorDefine.TestPackageName);

            // 初始化资源包
            var initParams = new EditorSimulateModeParameters();
            initParams.EditorFileSystemParameters = FileSystemParameters.CreateDefaultEditorFileSystemParameters(packageRoot);
            var initializeOp = package.InitializeAsync(initParams);
            yield return initializeOp;
            if (initializeOp.Status != EOperationStatus.Succeed)
                Debug.LogError(initializeOp.Error);
            Assert.AreEqual(EOperationStatus.Succeed, initializeOp.Status);

            // 请求资源版本
            var requetVersionOp = package.RequestPackageVersionAsync();
            yield return requetVersionOp;
            if (requetVersionOp.Status != EOperationStatus.Succeed)
                Debug.LogError(requetVersionOp.Error);
            Assert.AreEqual(EOperationStatus.Succeed, requetVersionOp.Status);

            // 更新资源清单
            var updateManifestOp = package.UpdatePackageManifestAsync(requetVersionOp.PackageVersion);
            yield return updateManifestOp;
            if (updateManifestOp.Status != EOperationStatus.Succeed)
                Debug.LogError(updateManifestOp.Error);
            Assert.AreEqual(EOperationStatus.Succeed, updateManifestOp.Status);
        }

        // 初始化RawPackage
        {
            string packageRoot = string.Empty;
#if UNITY_EDITOR
            packageRoot = UnityEditor.EditorPrefs.GetString(EFS_RAW_PACKAGE_ROOT_KEY);
#endif
            if (Directory.Exists(packageRoot) == false)
                throw new Exception($"Not found package root : {packageRoot}");

            var package = YooAssets.CreatePackage(AssetBundleCollectorDefine.RawPackageName);

            // 初始化资源包
            var initParams = new EditorSimulateModeParameters();
            initParams.EditorFileSystemParameters = FileSystemParameters.CreateDefaultEditorFileSystemParameters(packageRoot);
            var initializeOp = package.InitializeAsync(initParams);
            yield return initializeOp;
            if (initializeOp.Status != EOperationStatus.Succeed)
                Debug.LogError(initializeOp.Error);
            Assert.AreEqual(EOperationStatus.Succeed, initializeOp.Status);

            // 请求资源版本
            var requetVersionOp = package.RequestPackageVersionAsync();
            yield return requetVersionOp;
            if (requetVersionOp.Status != EOperationStatus.Succeed)
                Debug.LogError(requetVersionOp.Error);
            Assert.AreEqual(EOperationStatus.Succeed, requetVersionOp.Status);

            // 更新资源清单
            var updateManifestOp = package.UpdatePackageManifestAsync(requetVersionOp.PackageVersion);
            yield return updateManifestOp;
            if (updateManifestOp.Status != EOperationStatus.Succeed)
                Debug.LogError(updateManifestOp.Error);
            Assert.AreEqual(EOperationStatus.Succeed, updateManifestOp.Status);
        }
    }

    [UnityTest]
    public IEnumerator TestLoadAsyncTask()
    {
        var tester = new TestLoadPanel();
        yield return tester.RuntimeTester();
    }

    [UnityTest]
    public IEnumerator TestLoadAudio()
    {
        var tester = new TestLoadAudio();
        yield return tester.RuntimeTester();
    }

    [UnityTest]
    public IEnumerator TestLoadImage()
    {
        var tester = new TestLoadImage();
        yield return tester.RuntimeTester();
    }

    [UnityTest]
    public IEnumerator TestLoadPrefab()
    {
        var tester = new TestLoadPrefab();
        yield return tester.RuntimeTester();
    }

    [UnityTest]
    public IEnumerator TestLoadScene()
    {
        var tester = new TestLoadScene();
        yield return tester.RuntimeTester();
    }

    [UnityTest]
    public IEnumerator TestLoadScriptableObject()
    {
        var tester = new TestLoadScriptableObject();
        yield return tester.RuntimeTester();
    }

    [UnityTest]
    public IEnumerator TestLoadSpriteAtlas()
    {
        var tester = new TestLoadSpriteAtlas();
        yield return tester.RuntimeTester();
    }

    [UnityTest]
    public IEnumerator TestLoadRawFile()
    {
        var tester = new TestLoadRawFile();
        yield return tester.RuntimeTester();
    }

    [UnityTest]
    public IEnumerator TestLoadVideo()
    {
        var tester = new TestLoadVideo();
        yield return tester.RuntimeTester();
    }
}