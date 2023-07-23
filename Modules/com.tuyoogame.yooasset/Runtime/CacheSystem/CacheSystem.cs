﻿using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace YooAsset
{
	internal static class CacheSystem
	{
		private readonly static Dictionary<string, PatchBundle> _cachedDic = new Dictionary<string, PatchBundle>(1000);

		/// <summary>
		/// 初始化时的验证级别
		/// </summary>
		public static EVerifyLevel InitVerifyLevel { set; get; } = EVerifyLevel.Low;


		/// <summary>
		/// 清空所有数据
		/// </summary>
		public static void ClearAll()
		{
			_cachedDic.Clear();
		}

		/// <summary>
		/// 查询是否为验证文件
		/// 注意：被收录的文件完整性是绝对有效的
		/// </summary>
		public static bool IsCached(PatchBundle patchBundle)
		{
			string cacheKey = patchBundle.CacheKey;
			if (_cachedDic.ContainsKey(cacheKey))
			{
				string filePath = patchBundle.CachedFilePath;
				if (File.Exists(filePath))
				{
					return true;
				}
				else
				{
					_cachedDic.Remove(cacheKey);
					YooLogger.Error($"Cache file is missing : {filePath}");
					return false;
				}
			}
			else
			{
				return false;
			}
		}

		/// <summary>
		/// 缓存补丁包文件
		/// </summary>
		public static void CacheBundle(PatchBundle patchBundle)
		{
			string cacheKey = patchBundle.CacheKey;
			if (_cachedDic.ContainsKey(cacheKey) == false)
			{
				string filePath = patchBundle.CachedFilePath;
				YooLogger.Log($"Cache verify file : {filePath}");
				_cachedDic.Add(cacheKey, patchBundle);
			}
		}

		/// <summary>
		/// 验证补丁包文件
		/// </summary>
		public static EVerifyResult VerifyBundle(PatchBundle patchBundle, EVerifyLevel verifyLevel)
		{
			return VerifyContentInternal(patchBundle.CachedFilePath, patchBundle.FileSize, patchBundle.FileCRC, verifyLevel);
		}

		/// <summary>
		/// 验证并缓存本地文件
		/// </summary>
		public static EVerifyResult VerifyAndCacheLocalBundleFile(PatchBundle patchBundle, EVerifyLevel verifyLevel)
		{
			var verifyResult = VerifyContentInternal(patchBundle.CachedFilePath, patchBundle.FileSize, patchBundle.FileCRC, verifyLevel);
			if (verifyResult == EVerifyResult.Succeed)
				CacheBundle(patchBundle);
			return verifyResult;
		}

		/// <summary>
		/// 验证并缓存下载文件
		/// </summary>
		public static EVerifyResult VerifyAndCacheDownloadBundleFile(string tempFilePath, PatchBundle patchBundle, EVerifyLevel verifyLevel)
		{
			var verifyResult = VerifyContentInternal(tempFilePath, patchBundle.FileSize, patchBundle.FileCRC, verifyLevel);
			if (verifyResult == EVerifyResult.Succeed)
			{
				try
				{
					string destFilePath = patchBundle.CachedFilePath;
					if (File.Exists(destFilePath))
						File.Delete(destFilePath);

					FileInfo fileInfo = new FileInfo(tempFilePath);
					fileInfo.MoveTo(destFilePath);
				}
				catch (Exception)
				{
					verifyResult = EVerifyResult.FileMoveFailed;
				}

				if (verifyResult == EVerifyResult.Succeed)
				{
					CacheBundle(patchBundle);
				}
			}
			return verifyResult;
		}

		/// <summary>
		/// 验证文件完整性
		/// </summary>
		private static EVerifyResult VerifyContentInternal(string filePath, long fileSize, string fileCRC, EVerifyLevel verifyLevel)
		{
			try
			{
				if (File.Exists(filePath) == false)
					return EVerifyResult.FileNotExisted;

				// 先验证文件大小
				long size = FileUtility.GetFileSize(filePath);
				if (size < fileSize)
					return EVerifyResult.FileNotComplete;
				else if (size > fileSize)
					return EVerifyResult.FileOverflow;

				// 再验证文件CRC
				if (verifyLevel == EVerifyLevel.High)
				{
					string crc = HashUtility.FileCRC32(filePath);
					if (crc == fileCRC)
						return EVerifyResult.Succeed;
					else
						return EVerifyResult.FileCrcError;
				}
				else
				{
					return EVerifyResult.Succeed;
				}
			}
			catch (Exception)
			{
				return EVerifyResult.Exception;
			}
		}
	}
}