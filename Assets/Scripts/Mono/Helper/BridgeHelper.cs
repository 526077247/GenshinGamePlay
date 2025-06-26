using UnityEngine;

namespace TaoTie
{
    public static partial class BridgeHelper
    {
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
#if UNITY_WEBGL && !UNITY_EDITOR
            Vibrate();
#elif UNITY_ANDROID ||UNITY_IOS
            Handheld.Vibrate();
#else
            Log.Info("Vibrate Callback");
#endif
        }
    }
}