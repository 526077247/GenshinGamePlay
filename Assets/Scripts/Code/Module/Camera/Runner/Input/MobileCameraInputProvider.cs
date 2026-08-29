using UnityEngine;
using UnityEngine.EventSystems;

namespace TaoTie
{
    /// <summary>
    /// 移动端相机输入：缩放（捏合/单指纵向滚动）与视角旋转（单指拖拽）已由 InputManager
    /// 归一化到 MouseScrollWheel / MouseAxisX/Y，此处统一转发；双指捏合时抑制拖拽。
    /// </summary>
    public class MobileCameraInputProvider : ICameraInputProvider
    {
        public CameraInputIntent Current { get; private set; }

        public CameraInputIntent Tick()
        {
            bool twoFingers = Input.touchCount == 2;
            Current = new CameraInputIntent
            {
                ScrollDelta = InputManager.Instance.MouseScrollWheel,
                LookDelta = new Vector2(InputManager.Instance.MouseAxisX, InputManager.Instance.MouseAxisY),
                RotationAxis = 0f,
                MoveAxis = Vector2.zero,
                IsPointerDown = !twoFingers && Input.GetMouseButtonDown(0),
                IsPointerHeld = !twoFingers && Input.GetMouseButton(0),
                PointerPosition = InputManager.Instance.GetLastTouchPos(),
                IsPointerOverUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(),
                IsCursorUnLocked = CameraManager.Instance.CursorUnLockState > 0,
            };
            return Current;
        }

        public void Dispose()
        {
            Current = default;
        }
    }
}