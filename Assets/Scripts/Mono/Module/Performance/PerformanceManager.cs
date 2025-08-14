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
                    render.renderScale = 0.85f;
                }
                else
                {
                    render.renderScale = 0.75f;
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
             if(PlatformUtil.IsMobile())
                return DevicePerformanceLevel.Low;
             else
                return DevicePerformanceLevel.High;
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
                        return DevicePerformanceLevel.Low;
#elif UNITY_IOS
                    if(systemMemorySize >= 4000)
                        return DevicePerformanceLevel.High;
                    else
                        return DevicePerformanceLevel.Low;
#elif UNITY_STANDALONE_OSX
                    if (graphicsMemorySize >= 2000 && systemMemorySize >= 4000)
                        return DevicePerformanceLevel.High;
                    else
                        return DevicePerformanceLevel.Low;
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