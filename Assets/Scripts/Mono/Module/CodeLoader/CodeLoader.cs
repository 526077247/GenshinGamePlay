using System;
using System.Collections.Generic;
using UnityEditor;
using System.IO;
using System.Reflection;
using UnityEngine;
using YooAsset;
// using HybridCLR;
using UnityEngine.Networking;

namespace TaoTie
{
	public class CodeLoader
	{
		public static CodeLoader Instance = new CodeLoader();

		public Action Update;
		public Action LateUpdate;
		public Action FixedUpdate;
		public Action OnApplicationQuit;
		public Action<bool> OnApplicationFocus;

		private int assemblyVer;
		private Assembly assembly;

		public CodeMode CodeMode { get; set; }

		private MemoryStream assStream;
		private MemoryStream pdbStream;
		private byte[] optionBytes; //todo：dhe

		public static List<string> AllAotDllList
		{
			get
			{
				var res = new List<string>();
				res.AddRange(SystemAotDllList);
				res.AddRange(UserAotDllList);
				return res;
			}
		}

		public static string[] SystemAotDllList =
		{
			"mscorlib.dll",
			"System.dll",
			"System.Core.dll"
		};

		public static string[] UserAotDllList =
		{
			"Unity.ThirdParty.dll",
			"Unity.Mono.dll"
		};

		/// <summary>
		/// 为aot assembly加载原始metadata， 这个代码放aot或者热更新都行。
		/// 一旦加载后，如果AOT泛型函数对应native实现不存在，则自动替换为解释模式执行
		/// </summary>
// 		public void LoadMetadataForAOTAssembly(YooAsset.EPlayMode mode)
// 		{
// 			if(this.CodeMode != CodeMode.Wolong) return;
// 			// 注意，补充元数据是给AOT dll补充元数据，而不是给热更新dll补充元数据。
// 			// 热更新dll不缺元数据，不需要补充，如果调用LoadMetadataForAOTAssembly会返回错误
// // 			optionBytes = null;
// // 			if (mode != YooAsset.EPlayMode.EditorSimulateMode)
// // 			{
// // 				var op = YooAssets.LoadAssetSync($"{Define.AOTDir}Unity.Codes.dhao.bytes", TypeInfo<TextAsset>.Type);
// // 				TextAsset v = op.AssetObject as TextAsset;
// // 				optionBytes = v.bytes;
// // 				op.Release();
// // 			}
// // #if UNITY_EDITOR
// // 			else
// // 				optionBytes = (AssetDatabase.LoadAssetAtPath($"{Define.AOTDir}Unity.Codes.dhao.bytes", TypeInfo<TextAsset>.Type) as TextAsset)?.bytes;
// // #endif
// 			foreach (var aotDllName in AllAotDllList)
// 			{
// 				byte[] dllBytes = null;
// #if UNITY_EDITOR
// 				if (mode != YooAsset.EPlayMode.EditorSimulateMode)
// #endif
// 				{
// 					var op = YooAssetsMgr.Instance.LoadAssetSync<TextAsset>($"{Define.AOTLoadDir}{aotDllName}.bytes",YooAssetsMgr.DefaultName);
// 					TextAsset v = op.AssetObject as TextAsset;
// 					dllBytes = v.bytes;
// 					op.Release();
// 				}
// #if UNITY_EDITOR
// 				else
// 					dllBytes = (AssetDatabase.LoadAssetAtPath($"{Define.AOTDir}{aotDllName}.bytes", TypeInfo<TextAsset>.Type) as TextAsset).bytes;
// #endif
//
// 				var err = RuntimeApi.LoadMetadataForAOTAssembly(dllBytes,HomologousImageMode.SuperSet);
// 				Log.Info($"LoadMetadataForAOTAssembly:{aotDllName}. ret:{err}");
// 			}
//
// 		}
		public async ETTask Start()
		{
			if ((Define.Debug || Debug.isDebugBuild) && PlayerPrefs.GetInt("DEBUG_LoadFromUrl", 0) == 1)
			{
				CodeMode = CodeMode.LoadFromUrl;
			}

			switch (this.CodeMode)
			{
				case CodeMode.BuildIn:
				{
					foreach (var item in AppDomain.CurrentDomain.GetAssemblies())
					{
						if (item.FullName.Contains("Unity.Code"))
						{
							assembly = item;
							Log.Info("Get AOT Dll Success");
							break;
						}
					}

					if (assembly == null)
					{
						Log.Error("Get AOT Dll Fail, 请将Init上的CodeMode改为LoadDll，或者在打包选项上开启热更代码打AOT");
					}

					break;
				}
				// case CodeMode.Wolong:
				case CodeMode.LoadDll:
				{
					byte[] assBytes = null;
					byte[] pdbBytes = null;
					int version = PackageManager.Instance.Config.GetPackageMaxVersion(Define.DefaultName);
					if (this.assemblyVer != version) //dll版本不同
					{
						assembly = null;
						//和内置包版本一致，检查是否有可用AOT代码
						if (PackageManager.Instance.CdnConfig.BuildHotfixAssembliesAOT &&
						    PackageManager.Instance.BuildInPackageConfig.GetBuildInPackageVersion(Define.DefaultName)
						    == version)
						{
							foreach (var item in AppDomain.CurrentDomain.GetAssemblies())
							{
								if (item.FullName.Contains("Unity.Code"))
								{
									assembly = item;
									Log.Info("Get AOT Dll Success");
									break;
								}
							}
						}
					}

					//没有内置AOTdll，或者热更完dll版本不同
					if (this.assembly == null)
					{
						(assBytes, pdbBytes) = await GetBytes();
						if (assBytes != null)
						{
							assembly = Assembly.Load(assBytes, pdbBytes);
							Log.Info("Get Dll Success ! version=" + version);
						}
						else
						{
							Log.Error("Get Dll Fail");
						}
					}

					this.assemblyVer = version; //记录当前dll版本
					break;
				}
				case CodeMode.LoadFromUrl:
				{
					int version = PackageManager.Instance.Config.GetPackageMaxVersion(Define.DefaultName);
					var path = PlayerPrefs.GetString("DEBUG_LoadFromUrlPath", "http://127.0.0.1:8081/cdn/");
					path += $"Code{version}.dll.bytes";

					UnityWebRequest www = UnityWebRequest.Get(path);
					ETTask task = ETTask.Create();
					var op = www.SendWebRequest();
					op.completed += (a) => { task.SetResult(); };
					await task;
					if (www.result == UnityWebRequest.Result.Success)
					{
						assembly = Assembly.Load(www.downloadHandler.data);
					}
					else
					{
						Log.Error("下载dll失败： url: " + path);
					}

					break;
				}
			}

			if (assembly != null)
			{
				this.assemblyVer = PackageManager.Instance.Config.GetPackageMaxVersion(Define.DefaultName); //记录当前dll版本
				AssemblyManager.Instance.AddAssembly(GetType().Assembly);
				AssemblyManager.Instance.AddHotfixAssembly(assembly);
				IStaticAction start = new MonoStaticAction(assembly, "TaoTie.Entry", "Start");
				start.Run();
			}
			else
			{
				Log.Error("assembly == null");
			}
		}

		private async ETTask<(byte[], byte[])> GetBytes()
		{
			int version = PackageManager.Instance.Config.GetPackageMaxVersion(Define.DefaultName);
			byte[] assBytes = null, pdbBytes = null;
			if (PackageManager.Instance.PlayMode != EPlayMode.EditorSimulateMode)
			{
				var op = PackageManager.Instance.LoadAssetAsync<TextAsset>(
					$"{Define.HotfixLoadDir}Code{version}.dll.bytes", Define.DefaultName);
				await op.Task;
				assBytes = (op.AssetObject as TextAsset)?.bytes;
				op.Release();
				if (Define.Debug)
				{
					op = PackageManager.Instance.LoadAssetAsync<TextAsset>(
						$"{Define.HotfixLoadDir}Code{version}.pdb.bytes", Define.DefaultName);
					await op.Task;
					pdbBytes = (op.AssetObject as TextAsset)?.bytes;
					op.Release();
				}
			}
#if UNITY_EDITOR
			else
			{
				assBytes = (AssetDatabase.LoadAssetAtPath($"{Define.HotfixDir}Code{version}.dll.bytes",
						TypeInfo<TextAsset>.Type) as TextAsset)
					.bytes;
				pdbBytes = (AssetDatabase.LoadAssetAtPath($"{Define.HotfixDir}Code{version}.pdb.bytes",
						TypeInfo<TextAsset>.Type) as TextAsset)
					.bytes;
			}
#endif
			return (assBytes, pdbBytes);
		}

		public bool isReStart = false;

		public void ReStart()
		{
			isReStart = true;
		}
	}
}