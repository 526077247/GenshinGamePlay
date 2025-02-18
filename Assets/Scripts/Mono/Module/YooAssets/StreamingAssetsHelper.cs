using System.IO;
using UnityEngine;
using YooAsset;

namespace TaoTie
{
#if UNITY_EDITOR
	internal class PreprocessBuild : UnityEditor.Build.IPreprocessBuildWithReport
	{
		public int callbackOrder
		{
			get { return 0; }
		}

		/// <summary>
		/// 在构建应用程序前处理
		/// </summary>
		public void OnPreprocessBuild(UnityEditor.Build.Reporting.BuildReport report)
		{
			string saveFilePath = "Assets/Resources/BuildInPackageConfig.asset";
			if (File.Exists(saveFilePath))
				File.Delete(saveFilePath);

			string folderPath =
				$"{Application.streamingAssetsPath}/{YooAssetSettingsData.Setting.DefaultYooFolderName}";
			DirectoryInfo root = new DirectoryInfo(folderPath);
			if (root.Exists == false)
			{
				Debug.Log($"没有发现YooAsset内置目录 : {folderPath}");
				return;
			}

			var data = ScriptableObject.CreateInstance<BuildInPackageConfig>();
			data.PackageName = new ();
			data.PackageVer = new ();
			FileInfo[] files = root.GetFiles("*", SearchOption.AllDirectories);
			foreach (var fileInfo in files)
			{
				if (fileInfo.Name.StartsWith(YooAssetSettingsData.Setting.PackageManifestPrefix) && fileInfo.Name.EndsWith(".bytes"))
				{
					var temp = fileInfo.Name.Replace(".bytes", "");
					var vs = temp.Split("_");
					data.PackageName.Add(vs[1]);
					data.PackageVer.Add(int.Parse(vs[2]));
				}
			}

			if (Directory.Exists("Assets/Resources") == false)
				Directory.CreateDirectory("Assets/Resources");

			UnityEditor.AssetDatabase.CreateAsset(data, saveFilePath);
			UnityEditor.AssetDatabase.SaveAssets();
			UnityEditor.AssetDatabase.Refresh();
			Debug.Log($"一共{data.PackageName.Count}个内置文件，内置版本清单保存成功 : {saveFilePath}");
		}
	}
#endif
}