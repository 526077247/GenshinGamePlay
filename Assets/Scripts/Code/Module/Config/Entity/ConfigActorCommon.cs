using Nino.Serialization;
using UnityEngine;

namespace TaoTie
{
    [NinoSerialize]
    public partial class ConfigActorCommon
    {
        [NinoMember(1)][Tooltip("会影响相机机位")]
        public float Height = 1.5f;
        [NinoMember(2)]
        public float ModelHeight = 1.5f;
        [NinoMember(3)]
        public float Scale = 1;
        [NinoMember(3)][Tooltip("近景模式偏移高度")]
        public float NearFocusOffsetHeight = 0.5f;
    }
}