using System.Collections.Generic;
using Cinemachine;
using LitJson.Extensions;
using Nino.Serialization;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [NinoSerialize]
    public partial class BlendDefinition
    {
        [Tooltip("Shape of the blend curve")] [NinoMember(1)]
        public CinemachineBlendDefinition.Style style = CinemachineBlendDefinition.Style.EaseInOut;

        [Tooltip("Duration of the blend, in seconds")]
        [ShowIf("@style != CinemachineBlendDefinition.Style.Cut")]
        [NinoMember(2)]
        public float _time = 2;

        [ShowIf(nameof(style), CinemachineBlendDefinition.Style.Custom)] [JsonIgnore]
        public AnimationCurve customCurve;

        [NinoMember(3)]
        [HideInInspector]
        public SerializeAnimationCurveData CurveData
        {
            get
            {
                var res = new SerializeAnimationCurveData();
                res.AnimCurve = customCurve;
                return res;
            }
            set
            {
                customCurve = value.AnimCurve;
            }
        }
        [JsonIgnore]
        public float blendTime => this.style == CinemachineBlendDefinition.Style.Cut ? 0.0f : this._time;
        

        public CinemachineBlendDefinition ToCinemachineBlendDefinition()
        {
            return new CinemachineBlendDefinition()
            {
                m_Style = style,
                m_Time = blendTime,
                m_CustomCurve = customCurve
            };
        }
    }
}