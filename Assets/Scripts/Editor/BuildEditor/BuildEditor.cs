using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TaoTie
{
	public class PlatformTypeComparer : IEqualityComparer<PlatformType>
	{
		public static PlatformTypeComparer Instance = new PlatformTypeComparer();
		public bool Equals(PlatformType x, PlatformType y)
		{
			return x == y;          //x.Equals(y);  注意这里不要使用Equals方法，因为也会造成封箱操作
		}

		public int GetHashCode(PlatformType x)
		{
			return (int)x;
		}
	}
	public enum PlatformType:byte
	{
		None,
		Android,
		IOS,
		Windows,
		MacOS,
		Linux,
	}
	
	public enum BuildType:byte
	{
		Development,
		Release,
	}

	public class BuildEditor : EditorWindow
	{
		private const string settingAsset = "Assets/Scripts/Editor/BuildEditor/BuildSettings.asset";

		private PlatformType activePlatform;
		private PlatformType platformType;
		private bool clearFolder;
		private bool isBuildExe;
		private bool isContainAB;
		private bool isBuildAll;
		private bool isPackAtlas;
		private BuildType buildType;
		private BuildOptions buildOptions;
		private BuildAssetBundleOptions buildAssetBundleOptions = BuildAssetBundleOptions.None;
		private BuildSettings buildSettings;
		private int package;

		private BuildInConfig config;
		private PackageConfig packageConfig;
		[MenuItem("Tools/打包工具")]
		public static void ShowWindow()
		{
			GetWindow(typeof (BuildEditor));
		}

        private void OnEnable()
        {
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
#else
			activePlatform = PlatformType.None;
#endif
            platformType = activePlatform;

			if (!File.Exists(settingAsset))
            {
				buildSettings = new BuildSettings();
				AssetDatabase.CreateAsset(buildSettings, settingAsset);
            }
			else
			{
				buildSettings = AssetDatabase.LoadAssetAtPath<BuildSettings>(settingAsset);

				clearFolder = buildSettings.clearFolder;
				isBuildExe = buildSettings.isBuildExe;
				isContainAB = buildSettings.isContainAB;
				isBuildAll = buildSettings.isBuildAll;
				isPackAtlas = buildSettings.isPackAtlas;
				buildType = buildSettings.buildType;
				buildAssetBundleOptions = buildSettings.buildAssetBundleOptions;
			}
        }

        private void OnDisable()
        {
			SaveSettings();
        }

        private void OnGUI() 
		{
			if (this.config == null)
			{
				string jstr = File.ReadAllText("Assets/AssetsPackage/config.bytes");
				config = JsonHelper.FromJson<BuildInConfig>(jstr);
			}
			EditorGUILayout.LabelField("资源版本：" + this.config.Resver);
			EditorGUILayout.LabelField("代码版本：" + this.config.Dllver);
			if (GUILayout.Button("修改配置"))
			{
				System.Diagnostics.Process.Start("notepad.exe", "Assets/AssetsPackage/config.bytes");
			}
			if (GUILayout.Button("刷新配置"))
			{
				string jstr = File.ReadAllText("Assets/AssetsPackage/config.bytes");
				config = JsonHelper.FromJson<BuildInConfig>(jstr);
			}
			EditorGUILayout.LabelField("");
			int[] packageIndex = null;
			string[] packageNames = null;
			if (this.packageConfig == null)
			{
				string jstr = File.ReadAllText("Assets/AssetsPackage/packageConfig.bytes");
				packageConfig = JsonHelper.FromJson<PackageConfig>(jstr);
			}

			if (packageConfig.packageVer != null)
			{
				packageIndex = new int[packageConfig.packageVer.Count];
				packageNames = new string[packageConfig.packageVer.Count];
				int i = 0;
				foreach (var item in packageConfig.packageVer)
				{
					EditorGUILayout.LabelField(item.Key + "：" + item.Value);
					packageIndex[i] = i;
					packageNames[i] = item.Key;
					i++;
				}
			}
			if (GUILayout.Button("修改分包版本"))
			{
				Process.Start("notepad.exe", "Assets/AssetsPackage/packageConfig.bytes");
			}

			if (GUILayout.Button("刷新分包版本"))
			{
				string jstr = File.ReadAllText("Assets/AssetsPackage/packageConfig.bytes");
				packageConfig = JsonHelper.FromJson<PackageConfig>(jstr);
				if (packageConfig.packageVer != null)
				{
					packageIndex = new int[packageConfig.packageVer.Count];
					packageNames = new string[packageConfig.packageVer.Count];
					int i = 0;
					foreach (var item in packageConfig.packageVer)
					{
						packageIndex[i] = i;
						packageNames[i] = item.Key;
						i++;
					}
				}
			}
			
			EditorGUILayout.LabelField("打包平台:");
			this.platformType = (PlatformType)EditorGUILayout.EnumPopup(platformType);
            
			EditorGUILayout.LabelField("---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------");
			this.clearFolder = EditorGUILayout.Toggle("清理资源文件夹: ", clearFolder);
            this.isPackAtlas = EditorGUILayout.Toggle("是否需要重新打图集: ", isPackAtlas);
            this.isBuildExe = EditorGUILayout.Toggle("是否打包EXE(整包): ", this.isBuildExe);
            if (this.isBuildExe)
            {
	            this.isBuildAll = EditorGUILayout.Toggle("是否打包全量资源", this.isBuildAll);
            }
            this.buildType = (BuildType)EditorGUILayout.EnumPopup("BuildType: ", this.buildType);
			//EditorGUILayout.LabelField("BuildAssetBundleOptions(可多选):");
			//this.buildAssetBundleOptions = (BuildAssetBundleOptions)EditorGUILayout.EnumFlagsField(this.buildAssetBundleOptions);

			switch (buildType)
			{
				case BuildType.Development:
					this.buildOptions = BuildOptions.Development | BuildOptions.ConnectWithProfiler | BuildOptions.AllowDebugging;
					break;
				case BuildType.Release:
					this.buildOptions = BuildOptions.None;
					break;
			}

			GUILayout.Space(5);

			if (GUILayout.Button("开始打包"))
			{
				if (this.platformType == PlatformType.None)
				{
					ShowNotification(new GUIContent("请选择打包平台!"));
					return;
				}
				if (platformType != activePlatform)
                {
                    switch (EditorUtility.DisplayDialogComplex("警告!", $"当前目标平台为{activePlatform}, 如果切换到{platformType}, 可能需要较长加载时间", "切换", "取消", "不切换"))
                    {
						case 0:
							activePlatform = platformType;
							break;
						case 1:
							return;
                        case 2:
							platformType = activePlatform;
							break;
                    }
                }
				
				BuildHelper.Build(this.platformType, this.buildOptions, this.isBuildExe,this.clearFolder,this.isBuildAll,this.isPackAtlas);
			}
			if (packageNames != null)
			{
				EditorGUILayout.LabelField("---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------");

				package = EditorGUILayout.IntPopup("分包名： ", package, packageNames, packageIndex);
				if (GUILayout.Button("开始打分包"))
				{
					if (this.platformType == PlatformType.None)
					{
						ShowNotification(new GUIContent("请选择打包平台!"));
						return;
					}

					if (platformType != activePlatform)
					{
						switch (EditorUtility.DisplayDialogComplex("警告!",
							        $"当前目标平台为{activePlatform}, 如果切换到{platformType}, 可能需要较长加载时间", "切换", "取消", "不切换"))
						{
							case 0:
								activePlatform = platformType;
								break;
							case 1:
								return;
							case 2:
								platformType = activePlatform;
								break;
						}
					}

					BuildHelper.BuildPackage(platformType, packageNames[package]);
				}
			}

			GUILayout.Space(5);
		}

		private void SaveSettings()
		{
			buildSettings.clearFolder = clearFolder;
			buildSettings.isBuildExe = isBuildExe;
			buildSettings.isContainAB = isContainAB;
			buildSettings.buildType = buildType;
			buildSettings.isBuildAll = isBuildAll;
			buildSettings.isPackAtlas = isPackAtlas;
			buildSettings.buildAssetBundleOptions = buildAssetBundleOptions;

			EditorUtility.SetDirty(buildSettings);
			AssetDatabase.SaveAssets();
		}
	}
}
