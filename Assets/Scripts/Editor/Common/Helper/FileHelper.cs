using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace TaoTie
{
	public static class FileHelper
	{
		public static void GetAllFiles(List<string> files, string dir)
		{
			string[] fls = Directory.GetFiles(dir);
			foreach (string fl in fls)
			{
				files.Add(fl);
			}

			string[] subDirs = Directory.GetDirectories(dir);
			foreach (string subDir in subDirs)
			{
				GetAllFiles(files, subDir);
			}
		}
		
		public static void CleanDirectory(string dir)
		{
			foreach (string subdir in Directory.GetDirectories(dir))
			{
				Directory.Delete(subdir, true);		
			}

			foreach (string subFile in Directory.GetFiles(dir))
			{
				File.Delete(subFile);
			}
		}

		public static void CopyDirectory(string srcDir, string tgtDir)
		{
			DirectoryInfo source = new DirectoryInfo(srcDir);
			DirectoryInfo target = new DirectoryInfo(tgtDir);
	
			if (target.FullName.StartsWith(source.FullName, StringComparison.CurrentCultureIgnoreCase))
			{
				throw new Exception("父目录不能拷贝到子目录！");
			}
	
			if (!source.Exists)
			{
				return;
			}
	
			if (!target.Exists)
			{
				target.Create();
			}
	
			FileInfo[] files = source.GetFiles();
	
			for (int i = 0; i < files.Length; i++)
			{
				File.Copy(files[i].FullName, Path.Combine(target.FullName, files[i].Name), true);
			}
	
			DirectoryInfo[] dirs = source.GetDirectories();
	
			for (int j = 0; j < dirs.Length; j++)
			{
				CopyDirectory(dirs[j].FullName, Path.Combine(target.FullName, dirs[j].Name));
			}
		}
		public static void CopyFiles(string srcDir, string tgtDir, string[] ignore = null)
		{
			DirectoryInfo source = new DirectoryInfo(srcDir);
			DirectoryInfo target = new DirectoryInfo(tgtDir);
	
			if (target.FullName.StartsWith(source.FullName, StringComparison.CurrentCultureIgnoreCase))
			{
				throw new Exception("父目录不能拷贝到子目录！");
			}
	
			if (!source.Exists)
			{
				return;
			}
	
			if (!target.Exists)
			{
				target.Create();
			}
			DirectoryInfo[] dirs = source.GetDirectories();
			
			foreach (DirectoryInfo info in dirs)
			{
				CopyFiles(info.FullName, tgtDir, ignore);
			}

			FileInfo[] files = source.GetFiles();
	
			for (int i = 0; i < files.Length; i++)
			{
				if (ignore != null)
				{
					bool has = false;
					for (int j = 0; j < ignore.Length; j++)
					{
						if (files[i].FullName.Contains(ignore[j]))
						{
							has = true;
							break;
						}
					}
					if (has)
					{
						continue;
					}
				}
				CopyFile(files[i].FullName, Path.Combine(target.FullName, files[i].Name), true);
			}
		}
		public static void ReplaceExtensionName(string srcDir, string extensionName, string newExtensionName)
		{
			if (Directory.Exists(srcDir))
			{
				string[] fls = Directory.GetFiles(srcDir);

				foreach (string fl in fls)
				{
					if (fl.EndsWith(extensionName))
					{
						File.Move(fl, fl.Substring(0, fl.IndexOf(extensionName)) + newExtensionName);
						File.Delete(fl);
					}
				}

				string[] subDirs = Directory.GetDirectories(srcDir);

				foreach (string subDir in subDirs)
				{
					ReplaceExtensionName(subDir, extensionName, newExtensionName);
				}
			}
		}
		
		public static bool CopyFile(string sourcePath, string targetPath, bool overwrite)
		{
			string sourceText = null;
			string targetText = null;

			if (File.Exists(sourcePath))
			{
				sourceText = File.ReadAllText(sourcePath);
			}

			if (File.Exists(targetPath))
			{
				targetText = File.ReadAllText(targetPath);
			}

			if (sourceText != targetText && File.Exists(sourcePath))
			{
				File.Copy(sourcePath, targetPath, overwrite);
				return true;
			}

			return false;
		}
		/// <summary> 
		/// 检测指定目录是否存在 
		/// </summary> 
		/// <param name="directoryPath">目录的绝对路径</param>         
		public static bool IsExistDirectory(string directoryPath)
		{
			return Directory.Exists(directoryPath);
		}
		
		/// <summary> 
		/// 获取指定目录中所有文件列表 
		/// </summary> 
		/// <param name="directoryPath">指定目录的绝对路径</param>         
		public static string[] GetFileNames(string directoryPath)
		{
#if UNITY_WEB
        return null;
#endif
			//如果目录不存在，则抛出异常 
			if (!IsExistDirectory(directoryPath))
			{
				throw new FileNotFoundException();
			}
			//获取文件列表 
			return Directory.GetFiles(directoryPath);
		}
		
		/// <summary> 
		/// 获取指定目录及子目录中所有文件列表 (  *.png|*.txt|*.xml )
		/// </summary> 
		/// <param name="directoryPath">指定目录的绝对路径</param> 
		/// <param name="searchPattern">模式字符串，"*"代表0或N个字符，"?"代表1个字符。 
		/// 范例："Log*.xml"表示搜索所有以Log开头的Xml文件。</param> 
		/// <param name="isSearchChild">是否搜索子目录</param> 
		public static string[] GetFileNames(string directoryPath, string searchPattern, bool isSearchChild)
		{
#if UNITY_WEB
        return null;
#else
			int i;
			List<string> mList = new List<string>();
			string[] exts = searchPattern.Split('|');
			//如果目录不存在，则抛出异常 
			if (!IsExistDirectory(directoryPath))
			{
				//throw new FileNotFoundException();
				return mList.ToArray();
			}
			try
			{
				if (isSearchChild)
				{
					for (i = 0; i < exts.Length; i++)
					{
						mList.AddRange(Directory.GetFiles(directoryPath, exts[i], SearchOption.AllDirectories));
					}
					//return Directory.GetFiles(directoryPath, searchPattern, SearchOption.AllDirectories);
				}
				else
				{
					for (i = 0; i < exts.Length; i++)
					{
						mList.AddRange(Directory.GetFiles(directoryPath, exts[i], SearchOption.TopDirectoryOnly));
					}
					//return Directory.GetFiles(directoryPath, searchPattern, SearchOption.TopDirectoryOnly);
				}

				return mList.ToArray();
			}
			catch (IOException ex)
			{
				throw ex;
			}
#endif
		}
		
		public static bool SafeDeleteFile(string filePath)
		{
			try
			{
				if (string.IsNullOrEmpty(filePath))
				{
					return true;
				}

				if (!File.Exists(filePath))
				{
					return true;
				}
				File.SetAttributes(filePath, FileAttributes.Normal);
				File.Delete(filePath);
				return true;
			}
			catch (System.Exception ex)
			{
				Debug.LogError(string.Format("SafeDeleteFile failed! path = {0} with err: {1}", filePath, ex.Message));
				return false;
			}
		}
		
		static public string GetRelativeAssetsPath(string path)
		{
			return "Assets" + Path.GetFullPath(path).Replace(Path.GetFullPath(Application.dataPath), "").Replace('\\', '/');
		}
		
		public static void CreateArtSubFolder(string selectPath)
		{
			string[] ArtFolderNames = { "Animations", "Materials", "Models", "Textures", "Prefabs" };
			string[] UnitFolderNames = { "Animations", "Edit", "Materials", "Models", "Textures", "Prefabs" };
			string[] UIFolderNames = { "Animations", "Atlas", "DiscreteImages", "Prefabs" };
			Debug.Log(selectPath);
			if (Directory.Exists(selectPath))
			{
				var names = ArtFolderNames;
				selectPath.Replace("\\", "/");
				if (selectPath.Contains("UI/") || selectPath.Contains("UIHall/") || selectPath.Contains("UIGame/"))
				{
					names = UIFolderNames;
				}
				if (selectPath.Contains("Unit/"))
				{
					names = UnitFolderNames;
				}
				for (int j = 0; j < names.Length; j++)
				{
					string folderPath = Path.Combine(selectPath, names[j]);
					Debug.Log(folderPath);
					Directory.CreateDirectory(folderPath);
				}
			}
			else
			{
				Debug.Log(selectPath + " is not a directory");
			}
		}
	}
}
