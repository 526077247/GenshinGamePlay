﻿using System.IO;
using UnityEngine;
using UnityEditor;

namespace YooAsset.Editor
{
	public class PatchImportWindow : EditorWindow
	{
		static PatchImportWindow _thisInstance;

		[MenuItem("YooAsset/补丁包导入工具", false, 301)]
		static void ShowWindow()
		{
			if (_thisInstance == null)
			{
				_thisInstance = EditorWindow.GetWindow(typeof(PatchImportWindow), false, "补丁包导入工具", true) as PatchImportWindow;
				_thisInstance.minSize = new Vector2(800, 600);
			}
			_thisInstance.Show();
		}

		private string _patchManifestPath = string.Empty;

		private void OnGUI()
		{
			GUILayout.Space(10);
			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button("选择补丁包", GUILayout.MaxWidth(150)))
			{
				string resultPath = EditorUtility.OpenFilePanel("Find", "Assets/", "bytes");
				if (string.IsNullOrEmpty(resultPath))
					return;
				_patchManifestPath = resultPath;
			}
			EditorGUILayout.LabelField(_patchManifestPath);
			EditorGUILayout.EndHorizontal();

			if (string.IsNullOrEmpty(_patchManifestPath) == false)
			{
				if (GUILayout.Button("导入补丁包（全部文件）", GUILayout.MaxWidth(150)))
				{
					AssetBundleBuilderHelper.ClearStreamingAssetsFolder();
					CopyPatchFiles(_patchManifestPath);
				}
			}
		}

		private void CopyPatchFiles(string patchManifestFilePath)
		{
			string manifestFileName = Path.GetFileNameWithoutExtension(patchManifestFilePath);
			string outputDirectory = Path.GetDirectoryName(patchManifestFilePath);

			// 加载补丁清单
			byte[] bytesData = FileUtility.ReadAllBytes(patchManifestFilePath);
			PatchManifest patchManifest = PatchManifestTools.DeserializeFromBinary(bytesData);

			// 拷贝核心文件
			{
				string sourcePath = $"{outputDirectory}/{manifestFileName}.bytes";
				string destPath = $"{AssetBundleBuilderHelper.GetStreamingAssetsFolderPath()}/{manifestFileName}.bytes";
				EditorTools.CopyFile(sourcePath, destPath, true);
			}
			{
				string sourcePath = $"{outputDirectory}/{manifestFileName}.hash";
				string destPath = $"{AssetBundleBuilderHelper.GetStreamingAssetsFolderPath()}/{manifestFileName}.hash";
				EditorTools.CopyFile(sourcePath, destPath, true);
			}
			{
				string fileName = YooAssetSettingsData.GetPackageVersionFileName(patchManifest.PackageName);
				string sourcePath = $"{outputDirectory}/{fileName}";
				string destPath = $"{AssetBundleBuilderHelper.GetStreamingAssetsFolderPath()}/{fileName}";
				EditorTools.CopyFile(sourcePath, destPath, true);
			}

			// 拷贝文件列表
			int fileCount = 0;
			foreach (var patchBundle in patchManifest.BundleList)
			{
				fileCount++;
				string sourcePath = $"{outputDirectory}/{patchBundle.FileName}";
				string destPath = $"{AssetBundleBuilderHelper.GetStreamingAssetsFolderPath()}/{patchBundle.FileName}";
				EditorTools.CopyFile(sourcePath, destPath, true);
			}

			Debug.Log($"补丁包拷贝完成，一共拷贝了{fileCount}个资源文件");
			AssetDatabase.Refresh();
		}
	}
}