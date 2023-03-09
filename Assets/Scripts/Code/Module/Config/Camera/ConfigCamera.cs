using System.Collections.Generic;

using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    public abstract class ConfigCamera
    {
        public abstract CameraType type { get; }

        [PropertyOrder(int.MinValue)] [SerializeField] [Min(0)]
        private int _id;

        [PropertyOrder(int.MinValue + 1)] [LabelText("策划备注")] [SerializeField]
        private string _remark;

        [Range(-180, 180)] [SerializeField] private float _dutch;

        [Range(1, 179)] [SerializeField] private float _fov = 40;

        [Min(0.001f)] [SerializeField] private float _farClipPlane = 5000;

        [Min(0.001f)] [SerializeField] private float _nearClipPlane = 0.1f;

        [LabelText("显示光标")] [BoxGroup("其他设置")] [SerializeField]
        private bool _visibleCursor;

        [LabelText("是否允许摄像机震动")] [SerializeField] [BoxGroup("其他设置")]
        private bool _cameraShake;

        [LabelText("开启滚轮缩放")] [SerializeField] [BoxGroup("其他设置")]
        private bool _enableZoom;

        [LabelText("光标锁定模式")] [BoxGroup("其他设置")] [SerializeField]
        private CursorLockMode _mode = CursorLockMode.Locked;

        [ShowIf(nameof(enableZoom))] [Range(-1, 20)] [SerializeField] [BoxGroup("其他设置")]
        private float _zoomMin = -1;

        [ShowIf(nameof(enableZoom))] [Range(-1, 20)] [SerializeField] [BoxGroup("其他设置")]
        private float _zoomMax = 15;

        public int id => _id;
        public float fov => _fov;
        public float nearClipPlane => _nearClipPlane;
        public float farClipPlane => _farClipPlane;
        public float dutch => _dutch;
        public bool visibleCursor => _visibleCursor;
        public bool cameraShake => _cameraShake;
        public CursorLockMode mode => _mode;
        public bool enableZoom => _enableZoom;
        public float zoomMin => _zoomMin;
        public float zoomMax => _zoomMax;
    }
}