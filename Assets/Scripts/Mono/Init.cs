using System;
using System.Collections;
using Obfuz;
using Obfuz.EncryptionVM;
using YooAsset;
using UnityEngine;
using UnityEngine.UI;

namespace TaoTie
{

	public enum CodeMode
	{
		LoadDll = 1, //加载dll
		BuildIn = 2, //直接打进整包

		// Wolong = 3,
		LoadFromUrl = 4,
	}

	public class Init : MonoBehaviour
	{
		// 需要初始化Obfuz加密虚拟机后被混淆的代码才能正常运行。
		// 尽可能地早地初始化这个加密虚拟机。
		[ObfuzIgnore]
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
		private static void SetUpStaticSecret()
		{
			Debug.Log("SetUpStaticSecret begin");
			EncryptionService<DefaultStaticEncryptionScope>.Encryptor = new GeneratedEncryptionVirtualMachine(Resources.Load<TextAsset>("Obfuz/defaultStaticSecretKey").bytes);
			Debug.Log("SetUpStaticSecret end");
		}
		public CodeMode CodeMode = CodeMode.LoadDll;

		public EPlayMode PlayMode = EPlayMode.EditorSimulateMode;

		private bool IsInit = false;

		private WaitForEndOfFrame waitForEndOfFrame = new WaitForEndOfFrame();

		private async ETTask AwakeAsync()
		{
#if !UNITY_EDITOR
#if UNITY_WEBGL
			if (PlayMode != EPlayMode.WebPlayMode)
			{
				PlayMode = EPlayMode.WebPlayMode;
				Debug.LogError("Error PlayMode! " + PlayMode);
			}
#else
			if (PlayMode == EPlayMode.EditorSimulateMode || PlayMode == EPlayMode.WebPlayMode)
			{
				PlayMode = EPlayMode.HostPlayMode;
				Debug.LogError("Error PlayMode! " + PlayMode);
			}	
#endif
#endif
			InitUnitySetting();

			//设置时区
			TimeInfo.Instance.TimeZone = TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now).Hours;
			System.AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
			{
				Log.Error(e.ExceptionObject.ToString());
			};

			DontDestroyOnLoad(gameObject);

			ETTask.ExceptionHandler += Log.Error;

			Log.ILog = new UnityLogger();
			await InitSDK();
			await PackageManager.Instance.Init(PlayMode);
#if !UNITY_EDITOR
			if(this.CodeMode == CodeMode.BuildIn && !PackageManager.Instance.CdnConfig.BuildHotfixAssembliesAOT)
				this.CodeMode = CodeMode.LoadDll;
#endif
			RegisterManager();

			CodeLoader.Instance.CodeMode = this.CodeMode;
			IsInit = true;
			
			await CodeLoader.Instance.Start();
		}
		async ETTask InitSDK()
		{
			ETTask task = ETTask.Create(true);
#if MINIGAME_SUBPLATFORM_DOUYIN
			//build setting 添加包依赖
			TTSDK.TT.InitSDK((code, env) =>
			{
				task.SetResult();
				Log.Info("TT.InitSDK " + code);
			});
#elif MINIGAME_SUBPLATFORM_WEIXIN
#if UNITY_EDITOR
			task.SetResult();
#else
			//build setting 添加包依赖
			WeChatWASM.WX.InitSDK((code) =>
			{
				task.SetResult();
				Log.Info("WX.InitSDK " + code);
			});
#endif
#elif MINIGAME_SUBPLATFORM_KUAISHOU
#if UNITY_EDITOR
			task.SetResult();
#else
			//build setting 添加包依赖
			KSWASM.KS.InitSDK((code) =>
			{
				task.SetResult();
				Log.Info("KS.InitSDK " + code);
			});
#endif
#elif MINIGAME_SUBPLATFORM_MINIHOST
#if UNITY_EDITOR
			task.SetResult();
#else
			//build setting 添加包依赖
			minihost.TJ.InitSDK((code) =>
			{
				task.SetResult();
				Log.Info("minihost.InitSDK " + code);
			});
#endif
#else
			task.SetResult();
#endif
			await task;
		}
		private void Start()
		{
		    var canvasScaler = GameObject.Find("Canvas").GetComponent<CanvasScaler>();
            if (canvasScaler != null)
            {
                if ((float)Screen.width / Screen.height > Define.DesignScreenWidth / Define.DesignScreenHeight)
                    canvasScaler.matchWidthOrHeight = 1;
                else
                    canvasScaler.matchWidthOrHeight = 0;
            }
			AwakeAsync().Coroutine();
		}

		private void Update()
		{
			if (!IsInit) return;
			TimeInfo.Instance.Update();
			CodeLoader.Instance.Update?.Invoke();
			ManagerProvider.Update();
			if (CodeLoader.Instance.isReStart)
			{
				ReStart().Coroutine();
			}

			int count = UnityLifeTimeHelper.FrameFinishTask.Count;
			if (count > 0)
			{
				StartCoroutine(WaitFrameFinish());
			}
		}

		private IEnumerator WaitFrameFinish()
		{
			yield return waitForEndOfFrame;
			int count = UnityLifeTimeHelper.FrameFinishTask.Count;
			while (count-- > 0)
			{
				ETTask task = UnityLifeTimeHelper.FrameFinishTask.Dequeue();
				task.SetResult();
			}
		}

		public async ETTask ReStart()
		{
			CodeLoader.Instance.isReStart = false;
			Resources.UnloadUnusedAssets();
			await PackageManager.Instance.ForceUnloadAllAssets(Define.DefaultName);
			Resources.UnloadUnusedAssets();
			ManagerProvider.Clear();
			await PackageManager.Instance.UpdateConfig();
			//清两次，清干净
			GC.Collect();
			GC.Collect();
			Log.Debug("ReStart");

			RegisterManager();

			CodeLoader.Instance.OnApplicationQuit?.Invoke();
			await CodeLoader.Instance.Start();
		}

		private void RegisterManager()
		{
			ManagerProvider.RegisterManager<PerformanceManager>();
			ManagerProvider.RegisterManager<AssemblyManager>();
		}

		private void LateUpdate()
		{
			CodeLoader.Instance.LateUpdate?.Invoke();
			ManagerProvider.LateUpdate();
		}

		private void FixedUpdate()
		{
			CodeLoader.Instance.FixedUpdate?.Invoke();
			ManagerProvider.FixedUpdate();
		}

		private void OnApplicationQuit()
		{
			CodeLoader.Instance.OnApplicationQuit?.Invoke();
		}

		void OnApplicationFocus(bool hasFocus)
		{
			CodeLoader.Instance.OnApplicationFocus?.Invoke(hasFocus);
		}

		void OnApplicationPause(bool pauseStatus)
		{
			CodeLoader.Instance.OnApplicationFocus?.Invoke(!pauseStatus);
		}

		// 一些unity的设置项目
		void InitUnitySetting()
		{
			Input.multiTouchEnabled = false;
			//设置帧率
			QualitySettings.vSyncCount = 0;
			Application.runInBackground = true;
		}
	}
}