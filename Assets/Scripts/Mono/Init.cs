using System;
using System.Collections;
using System.Collections.Generic;
using YooAsset;
using System.Linq;
using System.Reflection;
using System.Threading;
using UnityEngine;
namespace TaoTie
{

	public enum CodeMode
	{
		LoadDll = 1,//加载dll
		BuildIn = 2,//直接打进整包
		// Wolong = 3,
		LoadFromUrl = 4,
	}
	
	public class Init: MonoBehaviour
	{
		public CodeMode CodeMode = CodeMode.LoadDll;

		public YooAsset.EPlayMode PlayMode = YooAsset.EPlayMode.EditorSimulateMode;

		private bool IsInit = false;

		private WaitForEndOfFrame waitForEndOfFrame = new WaitForEndOfFrame();
		private async ETTask AwakeAsync()
		{
			InitUnitySetting();
			
			//设置时区
			TimeInfo.Instance.TimeZone = TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now).Hours;
// #if ENABLE_IL2CPP
// 			if(this.CodeMode== CodeMode.LoadDll)
// 				this.CodeMode = CodeMode.Wolong;
// #endif
			System.AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
			{
				Log.Error(e.ExceptionObject.ToString());
			};

			DontDestroyOnLoad(gameObject);

			ETTask.ExceptionHandler += Log.Error;

			Log.ILog = new UnityLogger();
			
			await YooAssetsMgr.Instance.Init(PlayMode);
			
			RegisterManager();
			
			CodeLoader.Instance.CodeMode = this.CodeMode;
			IsInit = true;
			
			// CodeLoader.Instance.LoadMetadataForAOTAssembly(PlayMode);
			await CodeLoader.Instance.Start();
		}

		private void Start()
		{
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
			int count = WaitHelper.FrameFinishTask.Count;
			if (count > 0)
			{
				StartCoroutine(WaitFrameFinish());
			}
		}

		private IEnumerator WaitFrameFinish()
		{
			yield return waitForEndOfFrame;
			int count = WaitHelper.FrameFinishTask.Count;
			while (count-- > 0)
			{
				ETTask task = WaitHelper.FrameFinishTask.Dequeue();
				task.SetResult();
			}
		}

		public async ETTask ReStart()
		{
			CodeLoader.Instance.isReStart = false;
			Resources.UnloadUnusedAssets();
			YooAssetsMgr.Instance.ForceUnloadAllAssets(YooAssetsMgr.DefaultName);
			Resources.UnloadUnusedAssets();
			ManagerProvider.Clear();
			await YooAssetsMgr.Instance.UpdateConfig();
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
			ManagerProvider.RegisterManager<AssemblyManager>();
		}
		
		private void LateUpdate()
		{
			CodeLoader.Instance.LateUpdate?.Invoke();
		}
		
		private void FixedUpdate()
		{
			CodeLoader.Instance.FixedUpdate?.Invoke();
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
			Application.targetFrameRate = 60;
			Application.runInBackground = true;
		}
	}
}