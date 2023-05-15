using System.Collections.Generic;
using Nino.Serialization;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [NinoSerialize]
    public partial class ConfigCamera
    {

        [NinoMember(1)][PropertyOrder(int.MinValue)] [Min(0)]
        public int Id;
#if UNITY_EDITOR
        [NinoMember(2)][PropertyOrder(int.MinValue + 1)] [LabelText("策划备注")]
        public string Remarks;
#endif
        [NinoMember(3)]
        public ConfigCameraHeadPlugin HeadPlugin;
        
        [NinoMember(4)]
        public ConfigCameraBodyPlugin BodyPlugin;

        [NinoMember(5)] [Tooltip("顺序会影响最终效果")]
        public ConfigCameraOtherPlugin[] OtherPlugin;

        [NinoMember(6)]
        public ConfigCameraBleed Enter;

        [NinoMember(7)]
        public ConfigCameraBleed Level;
    }
}