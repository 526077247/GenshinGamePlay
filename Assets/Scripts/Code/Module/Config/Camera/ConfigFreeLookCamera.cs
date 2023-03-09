using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    public class ConfigFreeLookCamera : ConfigCamera
    {
        public override CameraType type => CameraType.FreeLookCameraPlugin;

        [SerializeField] [BoxGroup("TopRig")] [LabelText("Height")]
        private float _height1 = 4;

        [SerializeField] [BoxGroup("MiddleRig")] [LabelText("Height")]
        private float _height2 = 1.5f;

        [SerializeField] [BoxGroup("BottomRig")] [LabelText("Height")]
        private float _height3 = -1;

        [SerializeField] [BoxGroup("TopRig")] [LabelText("Radius")]
        private float _radius1 = 1f;

        [SerializeField] [BoxGroup("MiddleRig")] [LabelText("Radius")]
        private float _radius2 = 2.5f;

        [SerializeField] [BoxGroup("BottomRig")] [LabelText("Radius")]
        private float _radius3 = 0.5f;

        [SerializeField] [BoxGroup("Axis Control")] [Min(0)]
        private float _xSpeed = 1200;

        [SerializeField] [BoxGroup("Axis Control")] [Min(0)]
        private float _ySpeed = 15;

        [SerializeField] [LabelText("近镜模式开关")] [BoxGroup("近镜模式")]
        private bool _nearFocusEnable = true;

        [ShowIf("nearFocusEnable")]
        [BoxGroup("近镜模式")]
        [SerializeField]
        [LabelText("近镜模式开始距离")]
        [Range(0.1f, 12f)]
        [Tooltip("从这个距离开始偏移")]
        private float _nearFocusMaxDistance = 3f;

        [ShowIf("nearFocusEnable")]
        [BoxGroup("近镜模式")]
        [SerializeField]
        [LabelText("近镜模式结束距离")]
        [Range(0.1f, 12f)]
        [Tooltip("这个距离达到偏移最大值")]
        private float _nearFocusMinDistance = 1f;

        public float[] height => new[] {_height1, _height2, _height3};

        public float[] radius => new[] {_radius1, _radius2, _radius3};

        public float ySpeed => _ySpeed;
        public float xSpeed => _xSpeed;

        public bool nearFocusEnable => _nearFocusEnable;

        public float nearFocusMaxDistance => _nearFocusMaxDistance;
        public float nearFocusMinDistance => _nearFocusMaxDistance;
        
    }
}