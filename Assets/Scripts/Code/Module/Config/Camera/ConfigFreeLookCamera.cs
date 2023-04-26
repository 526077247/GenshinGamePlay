using System.Collections.Generic;
using LitJson.Extensions;
using Nino.Serialization;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [NinoSerialize]
    public partial class ConfigFreeLookCamera : ConfigCamera
    {
        public override CameraType Type => CameraType.FreeLookCameraPlugin;

        [NinoMember(20)] [BoxGroup("TopRig")] [LabelText("Height")]
        public float Height1 = 4;

        [NinoMember(21)] [BoxGroup("MiddleRig")] [LabelText("Height")]
        public float Height2 = 1.5f;

        [NinoMember(22)] [BoxGroup("BottomRig")] [LabelText("Height")]
        public float Height3 = -1;

        [NinoMember(23)] [BoxGroup("TopRig")] [LabelText("Radius")]
        public float Radius1 = 1f;

        [NinoMember(24)] [BoxGroup("MiddleRig")] [LabelText("Radius")]
        public float Radius2 = 2.5f;

        [NinoMember(25)] [BoxGroup("BottomRig")] [LabelText("Radius")]
        public float Radius3 = 0.5f;

        [NinoMember(26)] [BoxGroup("Axis Control")] [Min(0)]
        public float XSpeed = 1200;

        [NinoMember(27)] [BoxGroup("Axis Control")] [Min(0)]
        public float YSpeed = 15;

        [NinoMember(28)] [LabelText("近镜模式开关")] [BoxGroup("近镜模式")]
        public bool NearFocusEnable = true;

        [ShowIf("nearFocusEnable")]
        [BoxGroup("近镜模式")]
        [LabelText("近镜模式开始距离")]
        [Range(0.1f, 12f)]
        [Tooltip("从这个距离开始偏移")]
        [NinoMember(29)]
        public float NearFocusMaxDistance = 3f;

        [ShowIf("nearFocusEnable")]
        [BoxGroup("近镜模式")]
        [LabelText("近镜模式结束距离")]
        [Range(0.1f, 12f)]
        [Tooltip("这个距离达到偏移最大值")]
        [NinoMember(30)]
        public float NearFocusMinDistance = 1f;

        [JsonIgnore]
        public float[] Height => new[] {Height1, Height2, Height3};
        [JsonIgnore]
        public float[] Radius => new[] {Radius1, Radius2, Radius3};

    }
}