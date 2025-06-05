using UnityEngine;
using System.Runtime.InteropServices;

namespace TaoTie
{
    public static partial class BridgeHelper
    {
#if UNITY_WEBGL
        [DllImport("__Internal")]
        private static extern void CloseWindow();
        
        [DllImport("__Internal")]
        private static extern bool Vibrate();
#endif

        public static void Quit()
        {
#if UNITY_WEBGL
            CloseWindow();
#else
            Application.Quit();
#endif
        }

        public static void DoVibrate()
        {
#if UNITY_WEBGL
            Vibrate();
#elif UNITY_ANDROID ||UNITY_IOS
            Handheld.Vibrate();
#endif
        }
    }
}