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
				case CodeMode.LoadDll:
				{
					byte[] assBytes = null;
					byte[] pdbBytes= null;
					
					if (this.assemblyVer != YooAssetsMgr.Instance.Config.Dllver)//dll版本不同
					{
						this.assembly = null;
					}
					//没有内置AOTdll，或者热更完dll版本不同
					if (assembly == null)
					{
						GetBytes(out assBytes, out pdbBytes);
						assembly = Assembly.Load(assBytes, pdbBytes);
						Log.Info("Get Dll Success");
					}
					break;
				}
			}

			if (assembly != null)
			{
				this.assemblyVer = YooAssetsMgr.Instance.Config.Dllver;//记录当前dll版本
				AssemblyManager.Instance.AddAssembly(GetType().Assembly);
				AssemblyManager.Instance.AddHotfixAssembly(assembly);
				IStaticAction start = new MonoStaticAction(assembly, "TaoTie.Entry", "Start");
				start.Run();
			}
			else
			{
				Log.Error("assembly == null");
			}
			IsInit = true;
		}
		private void GetBytes(out byte[] assBytes,out byte[] pdbBytes)
		{
			assBytes = null;
			pdbBytes= null;
			if (YooAssetsMgr.Instance.PlayMode != YooAsset.EPlayMode.EditorSimulateMode)
			{
				var op = YooAssets.LoadAssetSync<TextAsset>(
					$"{Define.HotfixDir}Code{YooAssetsMgr.Instance.Config.Dllver}.dll.bytes");
				assBytes = (op.AssetObject as TextAsset).bytes;
				op.Release();
				op = YooAssets.LoadAssetSync<TextAsset>(
					$"{Define.HotfixDir}Code{YooAssetsMgr.Instance.Config.Dllver}.pdb.bytes");
				pdbBytes = (op.AssetObject as TextAsset).bytes;
				op.Release();
			}
#if UNITY_EDITOR
			else
			{
				string jstr = File.ReadAllText("Assets/AssetsPackage/config.bytes");
				var obj = JsonHelper.FromJson<BuildInConfig>(jstr);
				int version = obj.Dllver;
				assBytes = (AssetDatabase.LoadAssetAtPath($"{Define.HotfixDir}Code{version}.dll.bytes", TypeInfo<TextAsset>.Type) as TextAsset)
					.bytes;
				pdbBytes = (AssetDatabase.LoadAssetAtPath($"{Define.HotfixDir}Code{version}.pdb.bytes", TypeInfo<TextAsset>.Type) as TextAsset)
					.bytes;
			}
#endif
		}
		public bool isReStart = false;
		public void ReStart()
		{
			YooAssetsMgr.Instance.DefaultPackage.ForceUnloadAllAssets();
			Log.Debug("ReStart");
			isReStart = true;
		}
	}
}