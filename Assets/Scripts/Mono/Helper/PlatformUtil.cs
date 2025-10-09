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
        private static extern bool IsMobileWebGL();
#endif
        public static int GetIntPlatform()
        {
            return (int)Application.platform;
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
            return GetIntPlatform() == (int)RuntimePlatform.IPhonePlayer;
        }

        public static bool IsAndroid()
        {
            return GetIntPlatform() == (int)RuntimePlatform.Android;
        }

        public static bool IsWindows()
        {
            return GetIntPlatform() == (int)RuntimePlatform.WindowsPlayer;
        }
        
        public static bool IsWebGL()
        {
            return GetIntPlatform() == (int)RuntimePlatform.WebGLPlayer;
        }

        public static bool IsMobile()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            return IsMobileWebGL();
#else
            return IsAndroid() || IsIphone();
#endif
        }

        public static bool IsSimulator()
        {
#if UNITY_ANDROID
            return SystemInfo.graphicsDeviceID == 0 && SystemInfo.graphicsDeviceVendorID == 0;
#endif
            return false;
        }

        public static bool IsHarmony()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            return IsOpenHarmony();
#endif
            return false;
        }
        
        public static bool IsHuaWeiGroup()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            return IsHuaWeiHonor();
#endif
            return false;
        }
        
        public static bool IsWebGl1()
        {
#if UNITY_EDITOR
            bool webgl1 = true;
            if (UnityEditor.PlayerSettings.colorSpace == ColorSpace.Linear || UnityEditor.PlayerSettings.GetUseDefaultGraphicsAPIs(UnityEditor.BuildTarget.WebGL))
            {
                webgl1 = false;
            }
            else
            {
                UnityEngine.Rendering.GraphicsDeviceType[] graphicsAPIs = UnityEditor.PlayerSettings.GetGraphicsAPIs(UnityEditor.BuildTarget.WebGL);
                for (int i = 0; i < graphicsAPIs.Length; i++)
                {
                    if (graphicsAPIs[i] == UnityEngine.Rendering.GraphicsDeviceType.OpenGLES3)
                    {
                        webgl1 = false;
                        break;
                    }
                }
            }
            return webgl1;
#elif UNITY_WEBGL_1
            return true;
#else
            return false;
#endif
        }
    }
}