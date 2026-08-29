using UnityEngine;
using UnityEngine.EventSystems;

namespace TaoTie
{
    /// <summary>
    /// 桌面端相机输入：滚轮缩放 + 鼠标视角旋转 + 左键拖拽 + WASD 平移 + Q/E 旋转。
    /// 键位统一走 InputManager 的 GameKeyCode，支持键位重绑定。
    /// </summary>
    public class DesktopCameraInputProvider : ICameraInputProvider
    {
        public CameraInputIntent Current { get; private set; }

        public CameraInputIntent Tick()
        {
            var im = InputManager.Instance;
            Current = new CameraInputIntent
            {
                ScrollDelta = im.MouseScrollWheel,
                LookDelta = new Vector2(im.MouseAxisX, im.MouseAxisY),
                RotationAxis = (im.GetKey(GameKeyCode.Skill1) ? -1f : 0f) + (im.GetKey(GameKeyCode.Skill2) ? 1f : 0f),
                MoveAxis = MoveAxis(im),
                IsPointerDown = Input.GetMouseButtonDown(0),
                IsPointerHeld = Input.GetMouseButton(0),
                PointerPosition = Input.mousePosition,
                IsPointerOverUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(),
                IsCursorUnLocked = CameraManager.Instance.CursorUnLockState > 0,
            };
            return Current;
        }

        private Vector2 MoveAxis(InputManager im)
        {
            Vector2 axis = Vector2.zero;
            if (im.GetKey(GameKeyCode.MoveForward)) axis.y += 1f;
            if (im.GetKey(GameKeyCode.MoveBack)) axis.y -= 1f;
            if (im.GetKey(GameKeyCode.MoveLeft)) axis.x -= 1f;
            if (im.GetKey(GameKeyCode.MoveRight)) axis.x += 1f;
            return axis;
        }

        public void Dispose()
        {
            Current = default;
        }
    }
}