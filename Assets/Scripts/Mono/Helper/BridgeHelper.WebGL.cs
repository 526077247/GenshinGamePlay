using UnityEngine;
using System.Runtime.InteropServices;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace TaoTie
{
    public static partial class BridgeHelper
    {
#if UNITY_WEBGL
        [DllImport("__Internal")]
        private static extern void CloseWindow();
        
        [DllImport("__Internal")]
        private static extern bool Vibrate(int during);
        [DllImport("__Internal")]
        private static extern string StringReturnValueFunction();
        [DllImport("__Internal")]
        private static extern string NativeDialogPrompt(string title, string defaultValue);
        [DllImport("__Internal")]
        private static extern string SetupOverlayDialogHtml(string title , string defaultValue,string okBtnText,string cancelBtnText);

        [DllImport("__Internal")]
        private static extern bool IsOverlayDialogHtmlActive();
        [DllImport("__Internal")]
        private static extern bool IsOverlayDialogHtmlCanceled();
        [DllImport("__Internal")]
        private static extern string GetOverlayHtmlInputFieldValue();
        [DllImport("__Internal")]
        private static extern void HideUnityScreenIfHtmlOverlayCant();
        [DllImport("__Internal")]
        private static extern bool IsRunningOnEdgeBrowser();
        [DllImport("__Internal")]
        private static extern void OpenUploader();
        [DllImport("__Internal")]
        private static extern string GetImgData();
        [DllImport("__Internal")]
        private static extern void CopyTextToClipboard(string ptr);
#endif
        
        public static string OpenNativeStringDialog(string title, string defaultValue)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            return NativeDialogPrompt(title, defaultValue);
#else
            return defaultValue;
#endif
        }

        public static void SetUpOverlayDialog(string title, string defaultValue, string okBtnText, string cancelBtnText)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            if (Screen.fullScreen)
            {
                if( IsRunningOnEdgeBrowser() ){
                    Screen.fullScreen = false;
                }else{
                    HideUnityScreenIfHtmlOverlayCant();
                }
            }
            SetupOverlayDialogHtml(title, defaultValue,okBtnText,cancelBtnText);
#else
#endif
        }


        public static bool IsOverlayDialogActive()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            return IsOverlayDialogHtmlActive();
#else
            return false;
#endif
        }

        public static bool IsOverlayDialogCanceled()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            return IsOverlayDialogHtmlCanceled();
#else
            return false;
#endif
        }
        public static string GetOverlayDialogValue()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            return GetOverlayHtmlInputFieldValue();
#else
            return "";
#endif
        }

        public static void OnOpenUploader()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            OpenUploader();
#endif
        }

        public static string GetUploaderImgBase64()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            return GetImgData();
#else
            return null;
#endif
        }
        
        /// <summary>
        /// 获取Url参数
        /// </summary>
        /// <param name="para"></param>
        /// <returns></returns>
        public static string GetUrlParams(string para)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            var url = StringReturnValueFunction();
#else
            var url = "http://127.0.0.1:8081/TaoTie_googleplay/index.html?st=123";//测试
#endif
            var dic = ParseQueryString(url);
            if (dic.TryGetValue(para, out var str))
            {
                return str;
            }
            return null;
        }
        
        /// <summary>
        /// 如果URL中有参数可以进行参数的解释，
        /// </summary>
        /// <param name="queryString">查询字符串</param>
        /// <returns>键值对字典key是参数key，value是参数值</returns>
        private static Dictionary<string, string> ParseQueryString(string queryString)
        {
            if (queryString.StartsWith("http"))
            {
                var i = queryString.IndexOf("?");
                if (i > 0)
                {
                    queryString = queryString.Substring(i+1, queryString.Length - 1 - i);
                }
               
            }
            Dictionary<string, string> result = new Dictionary<string, string>();

            // 使用正则表达式匹配键值对
            string pattern = @"([^&=]+)=([^&]*)";
            MatchCollection matches = Regex.Matches(queryString, pattern);

            foreach (Match match in matches)
            {
                string key = Uri.UnescapeDataString(match.Groups[1].Value); // 解码键
                string value = Uri.UnescapeDataString(match.Groups[2].Value); // 解码值
                result[key] = value;
            }

            return result;
        }
    }
}