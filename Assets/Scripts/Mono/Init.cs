using System;
using System.Collections;
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
	}
	
	public class Init: MonoBehaviour
	{
		public CodeMode CodeMode = CodeMode.LoadDll;

		public YooAsset.EPlayMode PlayMode = YooAsset.EPlayMode.EditorSimulateMode;

		private bool IsInit = false;
		

		private async ETTask AwakeAsync()
		{
			InitUnitySetting();
			

			System.AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
			{
				Log.Error(e.ExceptionObject.ToString());
			};

			DontDestroyOnLoad(gameObject);

			ETTask.ExceptionHandler += Log.Error;

			Log.ILog = new UnityLogger();
			
#if !UNITY_EDITOR && !FORCE_UPDATE //编辑器模式下跳过更新
			Define.Networked = Application.internetReachability != NetworkReachability.NotReachable;
#endif

			await YooAssetsMgr.Instance.Init(PlayMode);
			
			CodeLoader.Instance.CodeMode = this.CodeMode;
			IsInit = true;
			CodeLoader.Instance.Start();
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
		}

		public async ETTask ReStart()
		{
			CodeLoader.Instance.isReStart = false;
			await YooAssetsMgr.Instance.UpdateConfig();
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