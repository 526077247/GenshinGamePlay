using UnityEngine;

namespace TaoTie
{
    /// <summary>
    /// 相机每帧输入快照：由 CameraManager 持有的 ICameraInputProvider 每帧统一产出，所有 Runner 共享消费、不读设备。
    /// </summary>
    public struct CameraInputIntent
    {
        /// <summary>缩放增量：滚轮 / 双指捏合 / 单指纵向滚动（平台统一口径）</summary>
        public float ScrollDelta;
        /// <summary>视角旋转增量：(x=水平, y=垂直)，第三人称/看向用</summary>
        public Vector2 LookDelta;
        /// <summary>平移输入轴：(x=左右, y=前后)，HexMap 键盘移动用</summary>
        public Vector2 MoveAxis;
        /// <summary>相机水平旋转轴：-1 / 0 / 1（HexMap Q/E 用）</summary>
        public float RotationAxis;
        /// <summary>本帧指针按下</summary>
        public bool IsPointerDown;
        /// <summary>指针按住</summary>
        public bool IsPointerHeld;
        /// <summary>指针屏幕位置</summary>
        public Vector2 PointerPosition;
        /// <summary>指针是否悬停于 UI</summary>
        public bool IsPointerOverUI;
        /// <summary>指针是否解锁（决定视角旋转是否启用）</summary>
        public bool IsCursorUnLocked;
    }
}