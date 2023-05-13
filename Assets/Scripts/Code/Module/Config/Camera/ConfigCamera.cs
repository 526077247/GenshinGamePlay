using System.Collections.Generic;
using Nino.Serialization;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    public abstract partial class ConfigCamera
    {
        public abstract CameraType Type { get; }

        [NinoMember(1)][PropertyOrder(int.MinValue)] [Min(0)]
        public int Id;
#if UNITY_EDITOR
        [NinoMember(2)][PropertyOrder(int.MinValue + 1)] [LabelText("策划备注")]
        public string Remarks;
#endif
        [NinoMember(3)][Range(-180, 180)] public float Dutch;

        [NinoMember(4)][Range(1, 179)] public float Fov = 40;

        [NinoMember(5)][Min(0.001f)] public float FarClipPlane = 5000;

        [NinoMember(6)][Min(0.001f)] public float NearClipPlane = 0.1f;

        [NinoMember(7)][LabelText("显示光标")] [BoxGroup("其他设置")]
        public bool VisibleCursor;

        [NinoMember(8)][LabelText("是否允许摄像机震动")] [BoxGroup("其他设置")]
        public bool CameraShake;

        [NinoMember(9)][LabelText("开启滚轮缩放")] [BoxGroup("其他设置")]
        public bool EnableZoom;

        [NinoMember(10)][LabelText("光标锁定模式")] [BoxGroup("其他设置")]
        public CursorLockMode Mode = CursorLockMode.Locked;

        [NinoMember(11)][ShowIf(nameof(EnableZoom))] [Range(-1, 20)] [BoxGroup("其他设置")]
        public float ZoomMin = 1f;

        [NinoMember(12)][ShowIf(nameof(EnableZoom))] [Range(-1, 20)] [BoxGroup("其他设置")]
        public float ZoomMax = 4;
        
        [NinoMember(13)][ShowIf(nameof(EnableZoom))] [Range(-1, 20)] [BoxGroup("其他设置")]
        public float ZoomDefault = 2.5f;
    }
}