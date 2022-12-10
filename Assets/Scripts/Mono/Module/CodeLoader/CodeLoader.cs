using System;
using System.Collections.Generic;
using UnityEditor;
using System.IO;
using System.Reflection;
using UnityEngine;
using YooAsset;

namespace TaoTie
{
	public class CodeLoader
	{
		public static CodeLoader Instance = new CodeLoader();

		public Action Update;
		public Action LateUpdate;
		public Action OnApplicationQuit;

		private Assembly assembly;
		
		public CodeMode CodeMode { get; set; }
		
		private MemoryStream assStream ;
		private MemoryStream pdbStream ;
		
		private CodeLoader()
		{
			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
			foreach (Assembly ass in assemblies)
			{
				AssemblyManager.Instance.AddHotfixAssembly(ass);
			}
		}
		
		public void Start()
		{
			AssetBundle ab = null;
			switch (this.CodeMode)
			{
				case CodeMode.BuildIn:
				{
					foreach (var item in AppDomain.CurrentDomain.GetAssemblies())
					{
						if (item.FullName.Contains("Unity.Code"))
						{
							assembly = item;
							Debug.Log("Get AOT Dll Success");
							break;
						}
					}
					break;
				}
				case CodeMode.LoadDll:
				{
					byte[] assBytes = null;
					byte[] pdbBytes= null;
					
					//先尝试直接加载AOT的dll
					if (YooAssetsMgr.Instance.IsDllBuildIn)
					{
						foreach (var item in AppDomain.CurrentDomain.GetAssemblies())
						{
							if (item.FullName.Contains("Unity.Code"))
							{
								assembly = item;
								Debug.Log("Get AOT Dll Success");
								break;
							}
						}
					}
					//再load
					if (assembly == null)
					{
						if (YooAssets.PlayMode != YooAssets.EPlayMode.EditorSimulateMode)
						{
							ab = YooAssetsMgr.Instance.SyncLoadAssetBundle("assets/assetspackage/code/hotfix.bundle");
							assBytes = ((TextAsset) ab.LoadAsset(
								$"{Define.HotfixDir}Code{YooAssetsMgr.Instance.Config.Dllver}.dll.bytes",
								typeof(TextAsset))).bytes;
							pdbBytes = ((TextAsset) ab.LoadAsset(
								$"{Define.HotfixDir}Code{YooAssetsMgr.Instance.Config.Dllver}.pdb.bytes",
								typeof(TextAsset))).bytes;
						}
#if UNITY_EDITOR
						else
						{
							string jstr = File.ReadAllText("Assets/AssetsPackage/config.bytes");
							var obj = JsonHelper.FromJson<BuildInConfig>(jstr);
							int version = obj.Dllver;
							assBytes = (AssetDatabase.LoadAssetAtPath($"{Define.HotfixDir}Code{version}.dll.bytes",
								typeof(TextAsset)) as TextAsset).bytes;
							pdbBytes = (AssetDatabase.LoadAssetAtPath($"{Define.HotfixDir}Code{version}.pdb.bytes",
								typeof(TextAsset)) as TextAsset).bytes;
						}
#endif
						assembly = Assembly.Load(assBytes, pdbBytes);
						Debug.Log("Get Dll Success");
					}

					
					break;
				}
			}

			if (assembly != null)
			{
				AssemblyManager.Instance.AddAssembly(assembly);
				IStaticAction start = new MonoStaticAction(assembly, "TaoTie.Entry", "Start");
				start.Run();
			}
			if (YooAssets.PlayMode != YooAssets.EPlayMode.EditorSimulateMode)
				ab?.Unload(true);

		}
		
		public bool isReStart = false;
		public void ReStart()
		{
			YooAssets.ForceUnloadAllAssets();
			Log.Debug("ReStart");
			isReStart = true;
		}
	}
}