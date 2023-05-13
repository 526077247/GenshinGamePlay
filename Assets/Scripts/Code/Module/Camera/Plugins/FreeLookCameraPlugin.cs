using Cinemachine;
using UnityEngine;

namespace TaoTie
{
    public class FreeLookCameraPlugin : CameraPlugin<ConfigFreeLookCamera, FreeLookCameraStateData>
    {
        private CinemachineCollider _collider;
        public CinemachineFreeLook camera => baseCamera as CinemachineFreeLook;
        public override CameraType type => CameraType.FreeLookCameraPlugin;

        private CinemachineComposer _topComposer;
        private CinemachineComposer _middleComposer;
        private CinemachineComposer _bottomComposer;

        private float _objHalfHeight;
        private FreeLookCameraStateData _stateData => stateData as FreeLookCameraStateData;
        private float lerpScroll;
        protected override void OnInitInternal(ConfigFreeLookCamera data)
        {
            base.OnInitInternal(data);
            baseCamera = obj.AddComponent<CinemachineFreeLook>();
            _collider = obj.AddComponent<CinemachineCollider>();
            baseCamera.AddExtension(_collider);
            _collider.m_Strategy = CinemachineCollider.ResolutionStrategy.PullCameraForward;
            _collider.m_CollideAgainst = LayerMask.GetMask("Default", "Terrain", "Stone");

            _topComposer = camera.GetRig(0).GetCinemachineComponent<CinemachineComposer>();
            _middleComposer = camera.GetRig(1).GetCinemachineComponent<CinemachineComposer>();
            _bottomComposer = camera.GetRig(2).GetCinemachineComponent<CinemachineComposer>();

            camera.m_XAxis.m_InvertInput = false;
            camera.m_YAxis.m_InvertInput = true;
            camera.m_YAxis.Value = 0.5f;
            camera.m_BindingMode = CinemachineTransposer.BindingMode.WorldSpace;
            baseCamera.Priority = 0;
        }

        public override void OnEnter()
        {
            base.OnEnter();
            if (stateData != null)
                ChangeState(stateData);
            else
                ChangeState(new FreeLookCameraStateData(defaultConfig));
        }

        public override void OnLevel(bool clearState)
        {
            base.OnLevel(clearState);
        }

        public override void OnRelease()
        {
            _bottomComposer = null;
            _middleComposer = null;
            _topComposer = null;
            baseCamera = null;
            _collider = null;
            base.OnRelease();
        }

        public override void Tick()
        {
            base.Tick();
            
            lerpScroll = Mathf.Lerp(lerpScroll,-InputManager.Instance.MouseScrollWheel,0.1f);
            if (lerpScroll != 0)
            {
                _stateData.Zoom += lerpScroll * GameTimerManager.Instance.GetDeltaTime() / 10f;
                _stateData.Zoom = Mathf.Clamp(_stateData.Zoom, _stateData.ZoomMin, _stateData.ZoomMax);
                UpdateDistance(_stateData);
            }

        }

        protected override void ChangeStateInternal(FreeLookCameraStateData stateData)
        {
            for (var i = 0; i < 3; i++)
            {
                camera.m_Orbits[i].m_Height = stateData.Height[i];
                camera.m_Orbits[i].m_Radius = stateData.Radius[i];
            }

            camera.m_Lens.FieldOfView = stateData.Fov;
            camera.m_Lens.Dutch = stateData.Dutch;
            camera.m_Lens.FarClipPlane = stateData.FarClipPlane;
            camera.m_Lens.NearClipPlane = stateData.NearClipPlane;
            camera.m_XAxis.m_MaxSpeed = stateData.XSpeed;
            camera.m_YAxis.m_MaxSpeed = stateData.YSpeed;

            UpdateDistance(stateData);

            base.ChangeStateInternal(stateData);
        }

        private void UpdateDistance(FreeLookCameraStateData stateData)
        {
            camera.m_Orbits[0].m_Height = camera.m_Orbits[1].m_Height+stateData.Zoom;
            camera.m_Orbits[1].m_Radius = stateData.Zoom;
            camera.m_Orbits[2].m_Height = camera.m_Orbits[1].m_Height-stateData.Zoom;
            NearFocusProcess(stateData);
        }

        /// <summary>
        /// 近景模式参数处理
        /// </summary>
        /// <param name="stateData"></param>
        private void NearFocusProcess(FreeLookCameraStateData stateData)
        {
            #region 近景模式

            float target = 0;
            if (stateData.NearFocusEnable && camera.m_Orbits[1].m_Radius <= stateData.NearFocusMaxDistance)
            {
                float dist = camera.m_Orbits[1].m_Radius - stateData.NearFocusMinDistance;
                if (dist <= 0)
                {
                    target = _objHalfHeight;
                }
                else if (dist < stateData.NearFocusMaxDistance)
                {
                    target = Mathf.Lerp(_objHalfHeight, 0f, dist / (stateData.NearFocusMaxDistance - stateData.NearFocusMinDistance));
                }

                target /= 2;
            }

            target += _objHalfHeight;
            var offset = _middleComposer.m_TrackedObjectOffset;
            if (Mathf.Abs(offset.y - target) > float.Epsilon)
            {
                offset.Set(offset.x, target, offset.z);
                _middleComposer.m_TrackedObjectOffset = offset;
                _topComposer.m_TrackedObjectOffset = offset;
                _bottomComposer.m_TrackedObjectOffset = offset;
            }

            #endregion
        }

        /// <summary>
        ///     设置FreeLookCamera的跟随物体
        /// </summary>
        /// <param name="followObj">角色</param>
        /// <param name="height">角色身高</param>
        /// <param name="cut">跳过过渡动画</param>
        public void SetFollowTransform(Transform followObj, float height, bool cut = false)
        {
            _objHalfHeight = height / 2;
            SetFollowTransform(followObj);
            SetLookAtTransform(followObj);
            if (stateData != null) //刷新配置
                NearFocusProcess(stateData as FreeLookCameraStateData);
            if (cut)
            {
                baseCamera.ForceCameraPosition(
                    followObj.position +
                    followObj.rotation * new Vector3(0, _stateData.Height[1], -_stateData.Radius[1]),
                    Quaternion.identity);
            }

        }
    }
}