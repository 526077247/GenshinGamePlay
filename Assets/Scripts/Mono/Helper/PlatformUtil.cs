using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TaoTie
{
    public class PlatformUtil
    {
#if UNITY_WEBGL
        [DllImport("__Internal")]
        private static extern int GetWebGLUA();
#endif
        /// <summary>
        /// 1：pc 2: android 3: ios
        /// </summary>
        /// <returns></returns>
        public static int GetSystemTypeWithWebGL()
        {
#if UNITY_ANDROID
            return 2;
#elif UNITY_IOS
            return 3;
#elif UNITY_WEBGL && !UNITY_EDITOR
            return GetWebGLUA();
#endif
            return 1;
        }
        public static int GetIntPlatform()
        {
            return (int)Application.platform;
        }
        public static int intPlatform
        {
            get
            {
                return GetIntPlatform();
            }
        }
        public static string GetStrPlatformIgnoreEditor()
        {
            if (IsIphone())
                return "ios";
            else if (IsAndroid())
                return "android";
            else if (IsWindows())
                return "pc";
#if UNITY_ANDROID
            return "android";
#elif UNITY_IOS
            return "ios";
#elif UNITY_WEBGL
            return "webgl";
#endif
            return "pc";
        }
        
        public static bool IsIphone()
        {
            return intPlatform == (int)RuntimePlatform.IPhonePlayer;
        }

        public static bool IsAndroid()
        {
            return intPlatform == (int)RuntimePlatform.Android;
        }

        public static bool IsWindows()
        {
            return intPlatform == (int)RuntimePlatform.WindowsPlayer;
        }

        public static bool IsMobile()
        {
            return IsAndroid() || IsIphone();
        }
    }
}