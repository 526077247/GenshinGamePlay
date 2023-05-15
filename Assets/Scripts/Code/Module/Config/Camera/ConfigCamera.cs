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
        [NinoMember(3)]
        public ConfigCameraHead Head;
        
        [NinoMember(4)]
        public ConfigCameraBody Body;
    }
}