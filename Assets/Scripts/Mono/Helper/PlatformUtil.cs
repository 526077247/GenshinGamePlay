using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TaoTie
{
    public class PlatformUtil
    {
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