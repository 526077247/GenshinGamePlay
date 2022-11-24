using System;
using System.Collections;
using YooAsset;
using System.Linq;
using System.Reflection;
using System.Threading;
using UnityEngine;
namespace TaoTie
{
	// 1 mono模式 2 ILRuntime模式 3 Wolong模式
	public enum CodeMode
	{
		Mono = 1,
	}
	
	public class Init: MonoBehaviour
	{
		public CodeMode CodeMode = CodeMode.Mono;

		public YooAssets.EPlayMode PlayMode = YooAssets.EPlayMode.EditorSimulateMode;

		private bool IsInit = false;
		

		private IEnumerator AwakeAsync()
		{
#if !UNITY_EDITOR && !FORCE_UPDATE //编辑器模式下跳过更新
			Define.Networked = Application.internetReachability != NetworkReachability.NotReachable;
#endif
			
#if UNITY_EDITOR
			// 编辑器下的模拟模式
			if (PlayMode == YooAssets.EPlayMode.EditorSimulateMode)
			{
				yield return YooAssetsMgr.Instance.Init(YooAssets.EPlayMode.EditorSimulateMode);
				var createParameters = new YooAssets.EditorSimulateModeParameters();
				createParameters.LocationServices = new AddressByPathLocationServices("Assets/AssetsPackage");
				//createParameters.SimulatePatchManifestPath = GetPatchManifestPath();
				yield return YooAssets.InitializeAsync(createParameters);
			}
			else
#endif
			// 单机运行模式
			if (PlayMode == YooAssets.EPlayMode.OfflinePlayMode)
			{
				yield return YooAssetsMgr.Instance.Init(YooAssets.EPlayMode.OfflinePlayMode);
				var createParameters = new YooAssets.OfflinePlayModeParameters();
				createParameters.LocationServices = new AddressByPathLocationServices("Assets/AssetsPackage");
				yield return YooAssets.InitializeAsync(createParameters);
			}
			// 联机运行模式
			else
			{
				yield return YooAssetsMgr.Instance.Init(YooAssets.EPlayMode.HostPlayMode);
				var createParameters = new YooAssets.HostPlayModeParameters();
				createParameters.LocationServices = new AddressByPathLocationServices("Assets/AssetsPackage");
				createParameters.DecryptionServices = new BundleDecryption();
				createParameters.ClearCacheWhenDirty = true;
				createParameters.DefaultHostServer = YooAssetsMgr.Instance.Config.RemoteCdnUrl+"/"+YooAssetsMgr.Instance.Config.Channel+"_"+PlatformUtil.GetStrPlatformIgnoreEditor();
				createParameters.FallbackHostServer = YooAssetsMgr.Instance.Config.RemoteCdnUrl2+"/"+YooAssetsMgr.Instance.Config.Channel+"_"+PlatformUtil.GetStrPlatformIgnoreEditor();
				createParameters.VerifyLevel = EVerifyLevel.High;
				yield return YooAssets.InitializeAsync(createParameters);

				// 先设置更新补丁清单
				yield return YooAssets.WeaklyUpdateManifestAsync(YooAssetsMgr.Instance.staticVersion);
			}

			InitUnitySetting();
			

			System.AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
			{
				Log.Error(e.ExceptionObject.ToString());
			};

			DontDestroyOnLoad(gameObject);

			ETTask.ExceptionHandler += Log.Error;

			Log.ILog = new UnityLogger();
			

			CodeLoader.Instance.CodeMode = this.CodeMode;
			IsInit = true;
			CodeLoader.Instance.Start();
			AssemblyManager.Instance.AddAssembly(GetType().Assembly);
		}

		private void Start()
		{
			StartCoroutine(AwakeAsync());
		}

		private void Update()
		{
			if (!IsInit) return;
			TimeInfo.Instance.Update();
			CodeLoader.Instance.Update?.Invoke();
			ManagerProvider.Update();
			if (CodeLoader.Instance.isReStart)
			{
				StartCoroutine(ReStart());
			}
		}

		public IEnumerator ReStart()
		{
			CodeLoader.Instance.isReStart = false;
			AssemblyManager.Instance.RemoveHotfixAssembly();
			yield return YooAssetsMgr.Instance.Init(YooAssets.PlayMode);
			// 先设置更新补丁清单
			yield return YooAssets.WeaklyUpdateManifestAsync(YooAssetsMgr.Instance.staticVersion);
			Log.Debug("ReStart");
			CodeLoader.Instance.OnApplicationQuit?.Invoke();
			CodeLoader.Instance.Start();
		}

		private void LateUpdate()
		{
			CodeLoader.Instance.LateUpdate?.Invoke();
		}

		private void OnApplicationQuit()
		{
			CodeLoader.Instance.OnApplicationQuit?.Invoke();
		}
		
		// 一些unity的设置项目
		void InitUnitySetting()
		{
			Input.multiTouchEnabled = false;
			//设置帧率
			QualitySettings.vSyncCount = 0;
			Application.targetFrameRate = 60;
			Application.runInBackground = true;
		}
	}
}