using UnityEngine;
using System.Runtime.InteropServices;

namespace TaoTie
{
    public static class BridgeHelper
    {
#if UNITY_WEBGL
        [DllImport("__Internal")]
        private static extern void CloseWindow();
#endif

        public static void Quit()
        {
#if UNITY_WEBGL
            CloseWindow();
#else
            Application.Quit();
#endif
        }
    }
}