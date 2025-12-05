using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace TaoTie
{
	public enum PlatformType:byte
	{
		None,
		Android,
		IOS,
		Windows,
		MacOS,
		Linux,
		WebGL,
	}
	
	public enum BuildType:byte
	{
		Development,
		Release,
	}
	
	public enum Mode:byte
	{
		本机开发,
		内网测试,
		外网测试,
		自定义服务器
	}

	public class BuildEditor : EditorWindow
	{
		private const string settingAsset = "Assets/Scripts/Editor/BuildEditor/BuildSettings.asset";

		private string channel;
		private string cdn;
		private Mode buildMode;
		private PlatformType activePlatform;
		private PlatformType platformType;

		private bool collectShaderVariant = false;
		private bool clearBuildCache = false;
		private bool clearReleaseFolder;
		private bool clearABFolder;
		private bool isBuildExe;
		private bool buildHotfixAssembliesAOT;
		private bool isContainsAb;
		private bool isBuildAll;
		private bool isPackAtlas;
		private BuildType buildType;
		private BuildOptions buildOptions;
		private BuildAssetBundleOptions buildAssetBundleOptions = BuildAssetBundleOptions.None;
		private BuildSettings buildSettings;
		private int package;

		private PackageConfig config;

		[MenuItem("Tools/打包工具")]
		public static void ShowWindow()
		{
			GetWindow(typeof (BuildEditor));
		}

        private void OnEnable()
        {
	        string jstr = File.ReadAllText("Assets/AssetsPackage/config.bytes");
	        var config = JsonHelper.FromJson<PackageConfig>(jstr);
	        config.DefaultPackageVersion = (int)(TimeInfo.Instance.ClientNow()/1000);
	        File.WriteAllText("Assets/AssetsPackage/config.bytes", JsonHelper.ToJson(config));
#if UNITY_ANDROID
			activePlatform = PlatformType.Android;
#elif UNITY_IOS
			activePlatform = PlatformType.IOS;
#elif UNITY_STANDALONE_WIN
	        activePlatform = PlatformType.Windows;
#elif UNITY_STANDALONE_OSX
			activePlatform = PlatformType.MacOS;
#elif UNITY_STANDALONE_LINUX
			activePlatform = PlatformType.Linux;
#elif UNITY_WEBGL
			activePlatform = PlatformType.WebGL;
#else
			activePlatform = PlatformType.None;
#endif
            platformType = activePlatform;

			if (!File.Exists(settingAsset))
            {
				buildSettings = CreateInstance<BuildSettings>();
				AssetDatabase.CreateAsset(buildSettings, settingAsset);
            }
			else
			{
				buildSettings = AssetDatabase.LoadAssetAtPath<BuildSettings>(settingAsset);

				if(buildSettings == null) return;
				clearBuildCache = buildSettings.clearBuildCache;
				clearReleaseFolder = buildSettings.clearReleaseFolder;
				clearABFolder = buildSettings.clearABFolder;
				isBuildExe = buildSettings.isBuildExe;
				buildHotfixAssembliesAOT = buildSettings.buildHotfixAssembliesAOT;
				isContainsAb = buildSettings.isContainsAb;
				isBuildAll = buildSettings.isBuildAll;
				isPackAtlas = buildSettings.isPackAtlas;
				buildType = buildSettings.buildType;
				buildAssetBundleOptions = buildSettings.buildAssetBundleOptions;
				channel = buildSettings.channel;
				buildMode = buildSettings.buildMode;
				collectShaderVariant = buildSettings.collectShaderVariant;
				cdn = buildSettings.cdn;
			}
        }

        private void OnDisable()
        {
			SaveSettings();
        }

        private void OnGUI() 
		{
			channel = EditorGUILayout.TextField("渠道：", channel);

			if (this.config == null)
			{
				string jstr = File.ReadAllText("Assets/AssetsPackage/config.bytes");
				config = JsonHelper.FromJson<PackageConfig>(jstr);
			}
			EditorGUILayout.LabelField("资源版本：" + this.config.GetPackageMaxVersion(Define.DefaultName));
			if (GUILayout.Button("修改配置"))
			{
				System.Diagnostics.Process.Start("notepad.exe", "Assets/AssetsPackage/config.bytes");
			}
			if (GUILayout.Button("刷新配置"))
			{
				string jstr = File.ReadAllText("Assets/AssetsPackage/config.bytes");
				config = JsonHelper.FromJson<PackageConfig>(jstr);
			}
			EditorGUILayout.LabelField("");

			EditorGUILayout.LabelField("打包平台:");
			this.platformType = (PlatformType)EditorGUILayout.EnumPopup(platformType);
            if (platformType != activePlatform)
            {
                switch (EditorUtility.DisplayDialogComplex("警告!", $"当前目标平台为{activePlatform}, 如果切换到{platformType}, 可能需要较长加载时间", "切换", "取消", "不切换"))
                {
                    case 0:
	                    activePlatform = platformType;
	                    BuildHelper.Switch(platformType);
	                    break;
                    case 1:
                        return;
                    case 2:
                        platformType = activePlatform;
                        break;
                }
            }
			EditorGUILayout.LabelField("---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------");
			this.clearBuildCache = EditorGUILayout.Toggle("清理构建缓存: ", clearBuildCache);
			this.clearReleaseFolder = EditorGUILayout.Toggle("清理打包输出文件夹: ", clearReleaseFolder);
			this.clearABFolder = EditorGUILayout.Toggle("清理AB缓存文件夹: ", clearABFolder);
			this.collectShaderVariant = EditorGUILayout.Toggle("是否重新收集shader变体: ", collectShaderVariant);
            this.isPackAtlas = EditorGUILayout.Toggle("是否需要重新打图集: ", isPackAtlas);
            this.isBuildAll = EditorGUILayout.Toggle("全量资源是否打进包:", this.isBuildAll);
            if (!this.isBuildAll)
            {
	            this.isContainsAb = EditorGUILayout.Toggle("是否同时打分包资源: ", this.isContainsAb);
            }

            var isil2cpp = PlayerSettings.GetScriptingBackend(BuildHelper.buildGroupmap[activePlatform]) ==
                           ScriptingImplementation.IL2CPP;
            if (isil2cpp)
            {
	            this.buildHotfixAssembliesAOT = true;
            }
            EditorGUI.BeginDisabledGroup(isil2cpp);
            this.buildHotfixAssembliesAOT = EditorGUILayout.Toggle(
	            new GUIContent("*热更代码是否打AOT:", "可以把热更代码同时打一份到il2cpp使首包代码运行速度达到最快，但是会增加构建代码大小以及代码占用内存大小（未使用热更方案时必须勾选此项, WebGL建议勾选此项）"),
	            this.buildHotfixAssembliesAOT, new GUILayoutOption[]{});
            EditorGUI.EndDisabledGroup();
            this.isBuildExe = EditorGUILayout.Toggle("是否打包EXE(整包): ", this.isBuildExe);
            if (this.isBuildExe)
            {
	            EditorGUILayout.LabelField("服务器:");
	            this.buildMode = (Mode)EditorGUILayout.EnumPopup(buildMode);
            }
            if (this.buildMode== Mode.自定义服务器)
            {
	            EditorGUILayout.LabelField("cdn地址:");
	            cdn = EditorGUILayout.TextArea(cdn);
            }
            
            this.buildType = (BuildType)EditorGUILayout.EnumPopup("BuildType: ", this.buildType);
			//EditorGUILayout.LabelField("BuildAssetBundleOptions(可多选):");
			//this.buildAssetBundleOptions = (BuildAssetBundleOptions)EditorGUILayout.EnumFlagsField(this.buildAssetBundleOptions);

			switch (buildType)
			{
				case BuildType.Development:
					this.buildOptions = BuildOptions.Development | BuildOptions.AllowDebugging
#if !UNITY_IOS
					                                             | BuildOptions.ConnectWithProfiler
#endif
						;
					break;
				case BuildType.Release:
					this.buildOptions = BuildOptions.None;
					break;
			}

			GUILayout.Space(5);

			if (GUILayout.Button("开始打包"))
			{
				if(isBuildExe && !buildHotfixAssembliesAOT)
				{
					foreach (var aotDllName in CodeLoader.AllAotDllList)
					{
						if (!File.Exists($"{Define.AOTDir}{aotDllName}.bytes"))
						{
							UnityEngine.Debug.LogError("没有生成补充元数据dll");
							return;
						}
					}
				}
				SaveSettings();
				BuildHelper.SetCdnConfig(channel, buildHotfixAssembliesAOT, (int) buildMode, cdn);
				if (this.platformType == PlatformType.None)
				{
					ShowNotification(new GUIContent("请选择打包平台!"));
					return;
				}

				if (clearBuildCache)
				{
					FileHelper.CleanDirectory("Library/Bee");
					FileHelper.CleanDirectory("Library/BuildCache");
					FileHelper.CleanDirectory("Library/BeeAssemblyBuilder");
				}

				if (collectShaderVariant)
				{
					BuildHelper.CollectSVC((res) =>
					{
						if (!res)
						{
							Debug.LogError("Failed to Collect shader Variants.");
						}
						else
						{
							BuildHelper.Build(platformType, buildOptions, isBuildExe,clearReleaseFolder, clearABFolder, buildHotfixAssembliesAOT,
								isBuildAll,isPackAtlas,isBuildAll || isContainsAb,channel);
						}
					});
				}
				else
				{
					BuildHelper.Build(platformType, buildOptions, isBuildExe,clearReleaseFolder, clearABFolder, buildHotfixAssembliesAOT,
						isBuildAll,isPackAtlas,isBuildAll || isContainsAb,channel);
				}
			}
		}

		private void SaveSettings()
		{
			if (buildSettings == null) return;
			buildSettings.clearBuildCache = clearBuildCache;
			buildSettings.clearReleaseFolder = clearReleaseFolder;
			buildSettings.clearABFolder = clearABFolder;
			buildSettings.isBuildExe = isBuildExe;
			buildSettings.buildHotfixAssembliesAOT = buildHotfixAssembliesAOT;
			buildSettings.isContainsAb = isContainsAb;
			buildSettings.buildType = buildType;
			buildSettings.isBuildAll = isBuildAll;
			buildSettings.isPackAtlas = isPackAtlas;
			buildSettings.buildAssetBundleOptions = buildAssetBundleOptions;
			buildSettings.channel = channel;
			buildSettings.cdn = cdn;
			buildSettings.buildMode = buildMode;
			buildSettings.collectShaderVariant = collectShaderVariant;
			EditorUtility.SetDirty(buildSettings);
			AssetDatabase.SaveAssets();
		}
	}
}
