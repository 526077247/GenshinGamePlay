using UnityEngine;

namespace TaoTie
{
    public partial class CameraManager
    {
        #region 光标

        /// <summary>
        ///     修改鼠标指针显示隐藏状态
        /// </summary>
        /// <param name="show"></param>
        public void ChangeCursorShowState(bool show)
        {
            if (curState.data.visibleCursor != show)
            {
                var data = curState.data.Clone();
                data.visibleCursor = show;
                ChangeCameraState(curState.id, data);
            }
        }

        /// <summary>
        ///     修改鼠标指针锁定状态
        /// </summary>
        /// <param name="lockMode"></param>
        public void ChangeCursorLockState(CursorLockMode lockMode)
        {
            if (curState.data.mode != lockMode)
            {
                var data = curState.data.Clone();
                data.mode = lockMode;
                ChangeCameraState(curState.id, data);
            }
        }

        /// <summary>
        ///     修改鼠标指针显示隐藏、锁定状态
        /// </summary>
        /// <param name="show"></param>
        /// <param name="lockMode"></param>
        public void ChangeCursorState(bool show, CursorLockMode lockMode)
        {
            if (curState.data.visibleCursor != show || curState.data.mode != lockMode)
            {
                var data = curState.data.Clone();
                data.visibleCursor = show;
                data.mode = lockMode;
                ChangeCameraState(curState.id, data);
            }
        }

        #endregion

        #region Common

        /// <summary>
        ///     回到上一个状态
        /// </summary>
        public void BackState()
        {
            if (lastState != null) ChangeCameraState(lastState.id, lastState.data);
        }

        /// <summary>
        ///     修改摄像机距离
        /// </summary>
        /// <param name="distance"></param>
        public void SetCameraDistance(float distance)
        {
            if (curCameraType == CameraType.FreeLookCameraPlugin)
            {
                var freeLookCameraPlugin = curCamera as FreeLookCameraPlugin;
                var data = curState.data.Clone() as FreeLookCameraStateData;
                if (data.enableZoom)
                {
                    bool flag = data.zoomMin < data.zoomMax;
                    if (distance < data.zoomMin == flag) distance = data.zoomMin;
                    if (distance > data.zoomMax == flag) distance = data.zoomMax;
                }

                data.height = freeLookCameraPlugin.defaultConfig.height;
                data.radius = freeLookCameraPlugin.defaultConfig.radius;
                data.height[0] = data.height[1] + (data.height[0] - data.height[1]) * distance;
                data.height[2] = data.height[1] + (data.height[2] - data.height[1]) * distance;
                data.radius[1] = distance;
                ChangeCameraState(curState.id, data);
            }
            else if (curCameraType == CameraType.VirtualCameraPlugin)
            {
                var curData = curState.data as VirtualCameraStateData;
                if (curData?.body == CinemachineBodyType.HardLockToTarget)
                    return;
                var data = curData.Clone() as VirtualCameraStateData;
                if (data.enableZoom)
                {
                    bool flag = data.zoomMin < data.zoomMax;
                    if (distance < data.zoomMin == flag) distance = data.zoomMin;
                    if (distance > data.zoomMax == flag) distance = data.zoomMax;
                }

                if (data.body == CinemachineBodyType.Transposer)
                    data.transposer.followOffset = new Vector3(data.transposer.followOffset.x,
                        data.transposer.followOffset.y, distance);
                else if (data.body == CinemachineBodyType.FramingTransposer)
                    data.framingTransposer.cameraDistance = distance;
                else
                    return;
                ChangeCameraState(curState.id, data);
            }
        }

        /// <summary>
        ///     切换摄像机
        /// </summary>
        /// <param name="id"></param>
        /// <param name="clearCurState">是否清除之前摄像机状态</param>
        public void ChangeCamera(int id, bool clearCurState = true)
        {
            ChangeCameraState(id, null, clearCurState);
        }

        #endregion

        #region VirtualCamera

        /// <summary>
        ///     设置VirtualCamera的坐标方向
        /// </summary>
        public void SetVirtualCameraFollowData(Vector3 position, Vector3 forward)
        {
            if (curCameraType == CameraType.VirtualCameraPlugin)
            {
                var data = curState.data.Clone() as VirtualCameraStateData;
                data.follow = position;
                data.lookAt = position + forward;
                ChangeCameraState(curState.id, data);
            }
        }

        /// <summary>
        ///     设置VirtualCamera的坐标方向
        /// </summary>
        public void SetVirtualCameraFollowData(Vector3 position, Quaternion rotation)
        {
            if (curCameraType == CameraType.VirtualCameraPlugin)
            {
                var data = curState.data.Clone() as VirtualCameraStateData;
                data.follow = position;
                data.lookAt = position + rotation * Vector3.forward;
                ChangeCameraState(curState.id, data);
            }
        }

        #endregion
    }
}