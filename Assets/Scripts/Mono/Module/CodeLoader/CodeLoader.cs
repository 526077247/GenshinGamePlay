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
		
		private int assemblyVer;
		private Assembly assembly;
		
		public CodeMode CodeMode { get; set; }
		
		private MemoryStream assStream ;
		private MemoryStream pdbStream ;

		public bool IsInit = false;

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
					
					//第一次启动用AOT
					if (!this.IsInit && YooAssetsMgr.Instance.IsDllBuildIn)
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
					else//热更完
					{
						if (this.assemblyVer != YooAssetsMgr.Instance.Config.Dllver)//dll版本不同
						{
							this.assembly = null;
						}
					}
					//没有内置AOTdll，或者热更完dll版本不同
					if (assembly == null)
					{
						GetBytes(out ab, out assBytes, out pdbBytes);
						assembly = Assembly.Load(assBytes, pdbBytes);
						Debug.Log("Get Dll Success");
						this.assemblyVer = YooAssetsMgr.Instance.Config.Dllver;//记录当前dll版本
					}
					break;
				}
			}

			if (assembly != null)
			{
				AssemblyManager.Instance.AddHotfixAssembly(assembly);
				IStaticAction start = new MonoStaticAction(assembly, "TaoTie.Entry", "Start");
				start.Run();
			}
			if (YooAssets.PlayMode != YooAssets.EPlayMode.EditorSimulateMode)
				ab?.Unload(true);
			IsInit = true;
		}
		private void GetBytes(out AssetBundle ab,out byte[] assBytes,out byte[] pdbBytes)
		{
			assBytes = null;
			pdbBytes= null;
			ab = null;
			if (YooAssets.PlayMode != YooAssets.EPlayMode.EditorSimulateMode)
			{
				ab = YooAssetsMgr.Instance.SyncLoadAssetBundle("assets/assetspackage/code/hotfix.bundle");
				assBytes = ((TextAsset) ab.LoadAsset($"{Define.HotfixDir}Code{YooAssetsMgr.Instance.Config.Dllver}.dll.bytes",
					typeof (TextAsset))).bytes;
				pdbBytes = ((TextAsset) ab.LoadAsset($"{Define.HotfixDir}Code{YooAssetsMgr.Instance.Config.Dllver}.pdb.bytes",
					typeof (TextAsset))).bytes;
			}
#if UNITY_EDITOR
			else
			{
				string jstr = File.ReadAllText("Assets/AssetsPackage/config.bytes");
				var obj = JsonHelper.FromJson<BuildInConfig>(jstr);
				int version = obj.Dllver;
				assBytes = (AssetDatabase.LoadAssetAtPath($"{Define.HotfixDir}Code{version}.dll.bytes", typeof (TextAsset)) as TextAsset)
					.bytes;
				pdbBytes = (AssetDatabase.LoadAssetAtPath($"{Define.HotfixDir}Code{version}.pdb.bytes", typeof (TextAsset)) as TextAsset)
					.bytes;
			}
#endif
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