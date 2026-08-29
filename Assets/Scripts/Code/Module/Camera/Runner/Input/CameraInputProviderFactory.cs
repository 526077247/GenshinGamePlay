namespace TaoTie
{
    /// <summary>
    /// 相机输入提供器工厂：按平台返回对应实现。
    /// </summary>
    public static class CameraInputProviderFactory
    {
        public static ICameraInputProvider Create()
        {
            return PlatformUtil.IsMobile()
                ? new MobileCameraInputProvider()
                : new DesktopCameraInputProvider();
        }
    }
}