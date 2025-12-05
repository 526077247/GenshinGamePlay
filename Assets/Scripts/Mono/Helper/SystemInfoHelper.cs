using UnityEngine;

namespace TaoTie
{
    public static class SystemInfoHelper
    {
#if MINIGAME_SUBPLATFORM_KUAISHOU
        private static KSWASM.SystemInfo systemInfo;
        private static KSWASM.SystemInfo SystemInfo
#elif MINIGAME_SUBPLATFORM_DOUYIN
        private static TTSDK.TTSystemInfo systemInfo;
        private static TTSDK.TTSystemInfo SystemInfo
#elif MINIGAME_SUBPLATFORM_WEIXIN
        private static WeChatWASM.SystemInfo systemInfo;
        private static WeChatWASM.SystemInfo SystemInfo
#elif MINIGAME_SUBPLATFORM_MINIHOST
        private static minihost.SystemInfo systemInfo;
        private static minihost.SystemInfo SystemInfo
#else
        //占位
        private static object systemInfo;
        private static object SystemInfo
#endif
        {
            get
            {
                if (systemInfo == null)
                {
#if MINIGAME_SUBPLATFORM_KUAISHOU
                    systemInfo = KSWASM.KS.GetSystemInfoSync();
#elif MINIGAME_SUBPLATFORM_DOUYIN
                    systemInfo = TTSDK.TT.GetSystemInfo();
#elif MINIGAME_SUBPLATFORM_WEIXIN
                    systemInfo = WeChatWASM.WX.GetSystemInfoSync();
#elif MINIGAME_SUBPLATFORM_MINIHOST
                    systemInfo = minihost.TJ.GetSystemInfoSync();
#else
                    systemInfo = 0;
#endif
                }
                return systemInfo;
            }
        }
        
        public static float screenHeight
        {
            get
            {
#if !UNITY_EDITOR && (MINIGAME_SUBPLATFORM_KUAISHOU || MINIGAME_SUBPLATFORM_DOUYIN || MINIGAME_SUBPLATFORM_WEIXIN || MINIGAME_SUBPLATFORM_MINIHOST)
                return (float)SystemInfo.screenHeight;
#else
                return Screen.height;
#endif
            }
        }
        
        public static float screenWidth
        {
            get
            {
#if !UNITY_EDITOR && (MINIGAME_SUBPLATFORM_KUAISHOU || MINIGAME_SUBPLATFORM_DOUYIN || MINIGAME_SUBPLATFORM_WEIXIN || MINIGAME_SUBPLATFORM_MINIHOST)
                return (float)SystemInfo.screenWidth;
#else
                return Screen.width;
#endif
            }
        }

        public static Rect safeArea
        {
            get
            {
#if !UNITY_EDITOR && (MINIGAME_SUBPLATFORM_KUAISHOU || MINIGAME_SUBPLATFORM_DOUYIN || MINIGAME_SUBPLATFORM_WEIXIN || MINIGAME_SUBPLATFORM_MINIHOST)
                var safeArea = SystemInfo.safeArea;
                return Rect.MinMaxRect((float)safeArea.left,(float)safeArea.top,(float)safeArea.right,(float)safeArea.bottom);
#else
                var screenSafeArea = Screen.safeArea;
                return Rect.MinMaxRect(screenSafeArea.xMin, Screen.height - screenSafeArea.yMax, screenSafeArea.xMax, Screen.height - screenSafeArea.yMin);
#endif
            }
        }
    }
}