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
        public override CameraType type => CameraType.FreeLookCameraPlugin;

        [NinoMember(20)] [BoxGroup("TopRig")] [LabelText("Height")]
        public float height1 = 4;

        [NinoMember(21)] [BoxGroup("MiddleRig")] [LabelText("Height")]
        public float height2 = 1.5f;

        [NinoMember(22)] [BoxGroup("BottomRig")] [LabelText("Height")]
        public float height3 = -1;

        [NinoMember(23)] [BoxGroup("TopRig")] [LabelText("Radius")]
        public float radius1 = 1f;

        [NinoMember(24)] [BoxGroup("MiddleRig")] [LabelText("Radius")]
        public float radius2 = 2.5f;

        [NinoMember(25)] [BoxGroup("BottomRig")] [LabelText("Radius")]
        public float radius3 = 0.5f;

        [NinoMember(26)] [BoxGroup("Axis Control")] [Min(0)]
        public float xSpeed = 1200;

        [NinoMember(27)] [BoxGroup("Axis Control")] [Min(0)]
        public float ySpeed = 15;

        [NinoMember(28)] [LabelText("近镜模式开关")] [BoxGroup("近镜模式")]
        public bool nearFocusEnable = true;

        [ShowIf("nearFocusEnable")]
        [BoxGroup("近镜模式")]
        [LabelText("近镜模式开始距离")]
        [Range(0.1f, 12f)]
        [Tooltip("从这个距离开始偏移")]
        [NinoMember(29)]
        public float nearFocusMaxDistance = 3f;

        [ShowIf("nearFocusEnable")]
        [BoxGroup("近镜模式")]
        [LabelText("近镜模式结束距离")]
        [Range(0.1f, 12f)]
        [Tooltip("这个距离达到偏移最大值")]
        [NinoMember(30)]
        public float nearFocusMinDistance = 1f;

        [JsonIgnore]
        public float[] height => new[] {height1, height2, height3};
        [JsonIgnore]
        public float[] radius => new[] {radius1, radius2, radius3};

    }
}