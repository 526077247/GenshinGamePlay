namespace TaoTie
{
    /// <summary>
    /// 相机输入提供器：由 CameraManager 统一创建/刷新/销毁，把平台输入归一化成 CameraInputIntent。
    /// 解耦 Runner 与具体输入方案（PC 键鼠 / 移动端触摸）。
    /// </summary>
    public interface ICameraInputProvider
    {
        CameraInputIntent Tick();
        CameraInputIntent Current { get; }
        void Dispose();
    }
}