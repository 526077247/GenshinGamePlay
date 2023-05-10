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
            if (curState.Data.VisibleCursor != show)
            {
                var data = curState.Data.Clone();
                data.VisibleCursor = show;
                ChangeCameraState(curState.Id, data);
            }
        }

        /// <summary>
        ///     修改鼠标指针锁定状态
        /// </summary>
        /// <param name="lockMode"></param>
        public void ChangeCursorLockState(CursorLockMode lockMode)
        {
            if (curState.Data.Mode != lockMode)
            {
                var data = curState.Data.Clone();
                data.Mode = lockMode;
                ChangeCameraState(curState.Id, data);
            }
        }

        /// <summary>
        ///     修改鼠标指针显示隐藏、锁定状态
        /// </summary>
        /// <param name="show"></param>
        /// <param name="lockMode"></param>
        public void ChangeCursorState(bool show, CursorLockMode lockMode)
        {
            if (curState.Data.VisibleCursor != show || curState.Data.Mode != lockMode)
            {
                var data = curState.Data.Clone();
                data.VisibleCursor = show;
                data.Mode = lockMode;
                ChangeCameraState(curState.Id, data);
            }
        }

        #endregion

        #region Common

        /// <summary>
        ///     回到上一个状态
        /// </summary>
        public void BackState()
        {
            if (lastState != null) ChangeCameraState(lastState.Id, lastState.Data);
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
                var data = curState.Data.Clone() as FreeLookCameraStateData;
                if (data.EnableZoom)
                {
                    bool flag = data.ZoomMin < data.ZoomMax;
                    if (distance < data.ZoomMin == flag) distance = data.ZoomMin;
                    if (distance > data.ZoomMax == flag) distance = data.ZoomMax;
                }

                data.Height = freeLookCameraPlugin.defaultConfig.Height;
                data.Radius = freeLookCameraPlugin.defaultConfig.Radius;
                data.Height[0] = data.Height[1] + (data.Height[0] - data.Height[1]) * distance;
                data.Height[2] = data.Height[1] + (data.Height[2] - data.Height[1]) * distance;
                data.Radius[1] = distance;
                ChangeCameraState(curState.Id, data);
            }
            else if (curCameraType == CameraType.VirtualCameraPlugin)
            {
                var curData = curState.Data as VirtualCameraStateData;
                if (curData?.Body == CinemachineBodyType.HardLockToTarget)
                    return;
                var data = curData.Clone() as VirtualCameraStateData;
                if (data.EnableZoom)
                {
                    bool flag = data.ZoomMin < data.ZoomMax;
                    if (distance < data.ZoomMin == flag) distance = data.ZoomMin;
                    if (distance > data.ZoomMax == flag) distance = data.ZoomMax;
                }

                if (data.Body == CinemachineBodyType.Transposer)
                    data.Transposer.followOffset = new Vector3(data.Transposer.followOffset.x,
                        data.Transposer.followOffset.y, distance);
                else if (data.Body == CinemachineBodyType.FramingTransposer)
                    data.FramingTransposer.cameraDistance = distance;
                else
                    return;
                ChangeCameraState(curState.Id, data);
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
        public void SetVirtualCameraFollowData(Vector3 position, Vector3 forward, bool cut = false)
        {
            if (curCameraType == CameraType.VirtualCameraPlugin)
            {
                var data = curState.Data.Clone() as VirtualCameraStateData;
                data.Follow = position;
                data.LookAt = position + forward;
                data.Cut = cut;
                ChangeCameraState(curState.Id, data);
            }
            else
            {
                Log.Error("相机类型不匹配");
            }
        }

        /// <summary>
        ///     设置VirtualCamera的坐标方向
        /// </summary>
        public void SetVirtualCameraFollowData(Vector3 position, Quaternion rotation, bool cut = false)
        {
            if (curCameraType == CameraType.VirtualCameraPlugin)
            {
                var data = curState.Data.Clone() as VirtualCameraStateData;
                data.Follow = position;
                data.LookAt = position + rotation * Vector3.forward;
                data.Cut = cut;
                ChangeCameraState(curState.Id, data);
            }
            else
            {
                Log.Error("相机类型不匹配");
            }
        }

        #endregion

        #region FreeLock

        /// <summary>
        ///     设置FreeLock
        /// </summary>
        public void SetFreeLockCameraFollow(Transform transform)
        {
            if (curCameraType == CameraType.FreeLookCameraPlugin)
            {
                var camera = curCamera as FreeLookCameraPlugin;
                camera?.SetFollowTransform(transform,0.75f,true);
            }
            else
            {
                Log.Error("相机类型不匹配");
            }
        }

        #endregion
        
        #region TrackCamera

        /// <summary>
        /// 使用指定相机播放相机动画
        /// </summary>
        /// <param name="cameraId"></param>
        /// <param name="route"></param>
        public void PlayTrackAnim(int cameraId, ConfigCameraRoute route)
        {
            if (configs.TryGetValue(cameraId, out var data) && data is ConfigTrackCamera config)
            {
                var state = new TrackCameraStateData(config);
                state.SmoothRoute = new CameraSmoothRoute(route);
                ChangeCameraState(cameraId, state, false);
            }
            else
            {
                Log.Error("相机不存在或类型不匹配");
            }
        }

        #endregion
    }
}