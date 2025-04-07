using Nino.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [NinoType(false)]
    public partial class ConfigActorCommon
    {
        [NinoMember(1)][LabelText("*Height")][Tooltip("会影响相机机位")]
        public float Height = 1.5f;
        [NinoMember(2)]
        public float ModelHeight = 1.5f;
        [NinoMember(3)]
        public float Scale = 1;
        [NinoMember(4)][LabelText("近景模式偏移高度")]
        public float NearFocusOffsetHeight = 0.5f;
    }
}