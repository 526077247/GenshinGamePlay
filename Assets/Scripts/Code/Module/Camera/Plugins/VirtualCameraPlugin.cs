using Cinemachine;
using UnityEngine;

namespace TaoTie
{
    public class VirtualCameraPlugin : CameraPlugin<ConfigVirtualCamera, VirtualCameraStateData>
    {
        private Transform _follow;
        private Transform _lookAt;
        public CinemachineVirtualCamera camera => baseCamera as CinemachineVirtualCamera;
        public override CameraType type => CameraType.VirtualCameraPlugin;

        protected override void OnInitInternal(ConfigVirtualCamera data)
        {
            base.OnInitInternal(data);
            baseCamera = obj.AddComponent<CinemachineVirtualCamera>();
            _follow = new GameObject($"{type}_Follow_{id}").transform;
            _lookAt = new GameObject($"{type}_LookAt_{id}").transform;
            _follow.transform.parent = CameraManager.Instance.root;
            _lookAt.transform.parent = CameraManager.Instance.root;
            SetFollowTransform(_follow);
            SetLookAtTransform(_lookAt);
            camera.AddCinemachineComponent<CinemachineHardLookAt>();
            baseCamera.Priority = 0;
        }

        public override void OnEnter()
        {
            base.OnEnter();
            if (stateData != null)
                ChangeState(stateData);
            else
                ChangeState(new VirtualCameraStateData(defaultConfig));
        }

        public override void OnLevel(bool clearState)
        {
            base.OnLevel(clearState);
        }

        public override void OnRelease()
        {
            baseCamera = null;
            Object.Destroy(_follow);
            Object.Destroy(_lookAt);
            base.OnRelease();
        }

        protected override void ChangeStateInternal(VirtualCameraStateData stateData)
        {
            base.ChangeStateInternal(stateData);
            camera.m_Lens.FieldOfView = stateData.Fov;
            camera.m_Lens.Dutch = stateData.Dutch;
            camera.m_Lens.FarClipPlane = stateData.FarClipPlane;
            camera.m_Lens.NearClipPlane = stateData.NearClipPlane;
            Vector3 pos = stateData.Follow;
            if (stateData.Body == CinemachineBodyType.Transposer)
            {
                var comp = camera.GetCinemachineComponent<CinemachineTransposer>();
                if (comp == null)
                    comp = camera.AddCinemachineComponent<CinemachineTransposer>();
                comp.enabled = true;
                comp.m_XDamping = stateData.Transposer.xDamping;
                comp.m_YDamping = stateData.Transposer.yDamping;
                comp.m_ZDamping = stateData.Transposer.zDamping;
                comp.m_YawDamping = stateData.Transposer.yawDamping;
                comp.m_FollowOffset = stateData.Transposer.followOffset;
                pos += stateData.Transposer.followOffset;
            }
            else if (stateData.Body == CinemachineBodyType.HardLockToTarget)
            {
                var comp = camera.GetCinemachineComponent<CinemachineHardLockToTarget>();
                if (comp == null)
                    comp = camera.AddCinemachineComponent<CinemachineHardLockToTarget>();
                comp.enabled = true;
                comp.m_Damping = stateData.HardLockToTarget.damping;
            }
            else if (stateData.Body == CinemachineBodyType.FramingTransposer)
            {
                var comp = camera.GetCinemachineComponent<CinemachineFramingTransposer>();
                if (comp == null)
                    comp = camera.AddCinemachineComponent<CinemachineFramingTransposer>();
                comp.enabled = true;
                comp.m_CameraDistance = stateData.FramingTransposer.cameraDistance;
                comp.m_TrackedObjectOffset = stateData.FramingTransposer.trackedObjectOffset;
            }
            else
            {
                var body = camera.GetCinemachineComponent(CinemachineCore.Stage.Body);
                if (body != null) body.enabled = false;
            }

            _follow.position = stateData.Follow;
            _lookAt.position = stateData.LookAt;
            if (stateData.Cut)
            {
                camera.ForceCameraPosition(pos, Quaternion.Euler(_lookAt.position - pos));
            }
        }
        
        /// <summary>
        /// 设置跟随物体
        /// </summary>
        /// <param name="followObj">角色</param>
        /// <param name="offset">偏移</param>
        /// <param name="cut">跳过过渡动画</param>
        public void SetFollowTransform(Transform followObj, Vector3 offset, bool cut = false)
        {
            var comp = camera.GetCinemachineComponent<CinemachineTransposer>();
            if (comp == null)
                comp = camera.AddCinemachineComponent<CinemachineTransposer>();
            comp.enabled = true;
            comp.m_FollowOffset = offset;
            SetFollowTransform(followObj);
            SetLookAtTransform(followObj);
            if (cut)
            {
                baseCamera.ForceCameraPosition(
                    followObj.position + offset,
                    Quaternion.identity);
            }
        }
    }
}