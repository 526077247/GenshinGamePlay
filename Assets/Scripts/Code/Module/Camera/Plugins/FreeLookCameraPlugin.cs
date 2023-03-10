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

        protected override void ChangeStateInternal(FreeLookCameraStateData stateData)
        {
            for (var i = 0; i < 3; i++)
            {
                camera.m_Orbits[i].m_Height = stateData.height[i];
                camera.m_Orbits[i].m_Radius = stateData.radius[i];
            }

            camera.m_Lens.FieldOfView = stateData.fov;
            camera.m_Lens.Dutch = stateData.dutch;
            camera.m_Lens.FarClipPlane = stateData.farClipPlane;
            camera.m_Lens.NearClipPlane = stateData.nearClipPlane;
            camera.m_XAxis.m_MaxSpeed = stateData.xSpeed;
            camera.m_YAxis.m_MaxSpeed = stateData.ySpeed;

            NearFocusProcess(stateData);

            base.ChangeStateInternal(stateData);
        }

        /// <summary>
        /// 近景模式参数处理
        /// </summary>
        /// <param name="stateData"></param>
        private void NearFocusProcess(FreeLookCameraStateData stateData)
        {
            #region 近景模式

            float target = 0;
            if (stateData.nearFocusEnable && camera.m_Orbits[1].m_Radius <= stateData.nearFocusMaxDistance)
            {
                float dist = camera.m_Orbits[1].m_Radius - stateData.nearFocusMinDistance;
                if (dist <= 0)
                {
                    target = _objHalfHeight;
                }
                else if (dist < stateData.nearFocusMaxDistance)
                {
                    target = Mathf.Lerp(_objHalfHeight, 0f, dist / stateData.nearFocusMaxDistance);
                }
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
                    followObj.rotation * new Vector3(0, _stateData.height[1], -_stateData.radius[1]),
                    Quaternion.identity);
            }

        }
    }
}