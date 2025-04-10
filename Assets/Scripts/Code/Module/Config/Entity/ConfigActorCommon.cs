using Nino.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [NinoType(false)]
    public partial class ConfigActorCommon
    {
        [NinoMember(1)][LabelText("*高度")][Tooltip("会影响相机机位")][MinValue(0.01f)]
        public float Height = 1.5f;
        [NinoMember(3)][LabelText("预制体缩放")]
        public float Scale = 1;
        [NinoMember(2)][LabelText("*模型高度")][Tooltip("影响寻路和避障")][MinValue(0.01f)]
        public float ModelHeight = 1.5f;
        [NinoMember(5)][LabelText("*模型半径")][Tooltip("影响寻路和避障")][MinValue(0.01f)]
        public float ModelRadius = 0.7f;
        [NinoMember(4)][LabelText("相机近景模式偏移高度")]
        public float NearFocusOffsetHeight = 0.5f;
    }
}