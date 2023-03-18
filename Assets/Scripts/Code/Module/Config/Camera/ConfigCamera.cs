using System.Collections.Generic;
using Nino.Serialization;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    public abstract partial class ConfigCamera
    {
        public abstract CameraType type { get; }

        [NinoMember(1)][PropertyOrder(int.MinValue)] [Min(0)]
        public int id;

        [NinoMember(2)][PropertyOrder(int.MinValue + 1)] [LabelText("策划备注")]
        public string remark;

        [NinoMember(3)][Range(-180, 180)] public float dutch;

        [NinoMember(4)][Range(1, 179)] public float fov = 40;

        [NinoMember(5)][Min(0.001f)] public float farClipPlane = 5000;

        [NinoMember(6)][Min(0.001f)] public float nearClipPlane = 0.1f;

        [NinoMember(7)][LabelText("显示光标")] [BoxGroup("其他设置")]
        public bool visibleCursor;

        [NinoMember(8)][LabelText("是否允许摄像机震动")] [BoxGroup("其他设置")]
        public bool cameraShake;

        [NinoMember(9)][LabelText("开启滚轮缩放")] [BoxGroup("其他设置")]
        public bool enableZoom;

        [NinoMember(10)][LabelText("光标锁定模式")] [BoxGroup("其他设置")]
        public CursorLockMode mode = CursorLockMode.Locked;

        [NinoMember(11)][ShowIf(nameof(enableZoom))] [Range(-1, 20)] [BoxGroup("其他设置")]
        public float zoomMin = -1;

        [NinoMember(12)][ShowIf(nameof(enableZoom))] [Range(-1, 20)] [BoxGroup("其他设置")]
        public float zoomMax = 15;
    }
}