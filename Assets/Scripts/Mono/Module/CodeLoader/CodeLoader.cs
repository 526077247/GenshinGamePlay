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
			switch (this.CodeMode)
			{
				case CodeMode.Mono:
				{
					byte[] assBytes = null;
					byte[] pdbBytes= null;
					AssetBundle ab = null;

					if (YooAssets.PlayMode != YooAssets.EPlayMode.EditorSimulateMode)
					{
						ab = YooAssetsMgr.Instance.SyncLoadAssetBundle("assets/assetspackage/code/hotfix.bundle");
						assBytes = ((TextAsset) ab.LoadAsset($"{Define.HotfixDir}Code{YooAssetsMgr.Instance.Config.Resver}.dll.bytes", typeof (TextAsset))).bytes;
						pdbBytes = ((TextAsset) ab.LoadAsset($"{Define.HotfixDir}Code{YooAssetsMgr.Instance.Config.Resver}.pdb.bytes", typeof (TextAsset))).bytes;
					}
#if UNITY_EDITOR
					else
					{
						string jstr = File.ReadAllText("Assets/AssetsPackage/config.bytes");
						var obj = JsonHelper.FromJson<BuildInConfig>(jstr);
						int version = obj.Resver;
						assBytes = (AssetDatabase.LoadAssetAtPath($"{Define.HotfixDir}Code{version}.dll.bytes", typeof (TextAsset)) as TextAsset).bytes;
						pdbBytes = (AssetDatabase.LoadAssetAtPath($"{Define.HotfixDir}Code{version}.pdb.bytes", typeof (TextAsset)) as TextAsset).bytes;
					}
#endif

					assembly = Assembly.Load(assBytes, pdbBytes);
					AssemblyManager.Instance.AddAssembly(assembly);
					IStaticMethod start = new MonoStaticMethod(assembly, "TaoTie.Entry", "Start");
					start.Run();
					if (YooAssets.PlayMode != YooAssets.EPlayMode.EditorSimulateMode)
						ab?.Unload(true);

					break;
				}
			}
		}
		
		public bool isReStart = false;
		public void ReStart()
		{
			YooAssets.ForceUnloadAllAssets();
			YooAssetsMgr.Instance.Init(YooAssets.PlayMode);
			Log.Debug("ReStart");
			AssemblyManager.Instance.RemoveHotfixAssembly();
			isReStart = true;
		}
	}
}