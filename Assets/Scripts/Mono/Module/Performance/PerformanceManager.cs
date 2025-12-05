using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace TaoTie
{
    public class PerformanceManager: IManager
    {
        public enum DevicePerformanceLevel
        {
            Low,
            Mid,
            High
        }
        
        public static PerformanceManager Instance;

        public DevicePerformanceLevel Level { get; private set; }
        public void Init()
        {
            Instance = this;
            Level = GetDevicePerformanceLevel();
            var render = GraphicsSettings.renderPipelineAsset as UniversalRenderPipelineAsset;
            if (render!= null)
            {
                if (Level == DevicePerformanceLevel.High)
                {
                    render.renderScale = PlatformUtil.IsMobile() ? 0.85f : 1f;
                    render.colorGradingMode = ColorGradingMode.HighDynamicRange;
                    render.colorGradingLutSize = 32;
                    render.hdrColorBufferPrecision = HDRColorBufferPrecision._64Bits;
                    render.msaaSampleCount = 4;
                }
                else
                {
                    render.renderScale = Level == DevicePerformanceLevel.Mid ? 0.7f : 0.55f;
                    render.colorGradingMode = ColorGradingMode.LowDynamicRange;
                    render.colorGradingLutSize = 16;
                    render.hdrColorBufferPrecision = HDRColorBufferPrecision._32Bits;
                    render.msaaSampleCount = 2;
                }
            }
            SetLowFrame();
        }

        public void Destroy()
        {
            Instance = null;
        }

        public void SetFrame(int val)
        {
            Application.targetFrameRate = val;
        }

        public void SetHighFrame()
        {
            SetFrame(60);
        }

        public void SetLowFrame()
        {
            if (Level == DevicePerformanceLevel.High)
            {
                SetFrame(60);
            }
            else
            {
                SetFrame(30);
            }
        }
        
        /// <summary>
        /// 获取设备性能评级
        /// </summary>
        /// <returns>性能评级</returns>
        public DevicePerformanceLevel GetDevicePerformanceLevel()
        {
#if UNITY_WEBGL
#if MINIGAME_SUBPLATFORM_DOUYIN
            //PC 直播伴侣小游戏
            if (TTSDK.TT.GetSystemInfo().platform == "windows")
            {
                return DevicePerformanceLevel.High;
            }
#endif
            if (PlatformUtil.IsMobile())
            {
#if MINIGAME_SUBPLATFORM_DOUYIN && !UNITY_EDITOR
                return TTSDK.TT.GetSystemInfo().deviceScore.gpu > 10 && TTSDK.TT.GetSystemInfo().deviceScore.cpu > 10
                    ? DevicePerformanceLevel.Mid
                    : DevicePerformanceLevel.Low;
#elif MINIGAME_SUBPLATFORM_WEIXIN && !UNITY_EDITOR
                var benchmarkLevel = WeChatWASM.WX.GetSystemInfoSync().benchmarkLevel;
                if (benchmarkLevel >= 1)
                {
                    return benchmarkLevel > 30 ? DevicePerformanceLevel.Mid : DevicePerformanceLevel.Low;
                }
                return PlatformUtil.IsIphone() ? DevicePerformanceLevel.Mid : DevicePerformanceLevel.Low;
#else
                return PlatformUtil.IsIphone() ? DevicePerformanceLevel.Mid : DevicePerformanceLevel.Low;
#endif
            }
            else
            {
                return DevicePerformanceLevel.High;
            }
#endif
            if (SystemInfo.graphicsDeviceVendorID == 32902)
            {
                //集显
                return DevicePerformanceLevel.Low;
            }
            else //NVIDIA系列显卡（N卡）和AMD系列显卡
            {
                //根据目前硬件配置三个平台设置了不一样的评判标准（仅个人意见）
                //CPU核心数
#if UNITY_EDITOR || UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX
                if (SystemInfo.processorCount < 1)
#elif UNITY_IOS
                if (SystemInfo.processorCount <= 1)
#elif UNITY_ANDROID
                if (SystemInfo.processorCount <= 2)
#else
                if (SystemInfo.processorCount <= 2)
#endif
                {
                    //CPU核心数<=2判定为低端
                    return DevicePerformanceLevel.Low;
                }
                else
                {
                    //显存
                    int graphicsMemorySize = SystemInfo.graphicsMemorySize;
                    //内存
                    int systemMemorySize = SystemInfo.systemMemorySize;
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
                    if (graphicsMemorySize >= 2000 && systemMemorySize >= 4000)
                        return DevicePerformanceLevel.High;
                    else
                        return DevicePerformanceLevel.Mid;
#elif UNITY_IOS
                    if(systemMemorySize >= 4000)
                        return DevicePerformanceLevel.High;
                    else
                        return DevicePerformanceLevel.Mid;
#elif UNITY_STANDALONE_OSX
                    if (graphicsMemorySize >= 2000 && systemMemorySize >= 4000)
                        return DevicePerformanceLevel.High;
                    else
                        return DevicePerformanceLevel.Mid;
#elif UNITY_ANDROID
                    if (graphicsMemorySize >= 2000 && systemMemorySize >= 4000)
                        return DevicePerformanceLevel.High;
                    else
                        return DevicePerformanceLevel.Low;
#endif
                }
                return DevicePerformanceLevel.Low;
            }
        }
    }
}