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
        public CinemachineBlendDefinition.Style Style = CinemachineBlendDefinition.Style.EaseInOut;

        [Tooltip("Duration of the blend, in seconds")]
        [ShowIf("@"+nameof(Style)+" != CinemachineBlendDefinition.Style.Cut")]
        [NinoMember(2)]
        public float Time = 2;

        [ShowIf(nameof(Style), CinemachineBlendDefinition.Style.Custom)] [JsonIgnore]
        public AnimationCurve CustomCurve;

        [NinoMember(3)]
        [HideInInspector]
        public SerializeAnimationCurveData CurveData
        {
            get
            {
                var res = new SerializeAnimationCurveData();
                res.AnimCurve = CustomCurve;
                return res;
            }
            set
            {
                CustomCurve = value.AnimCurve;
            }
        }
        [JsonIgnore]
        public float BlendTime => this.Style == CinemachineBlendDefinition.Style.Cut ? 0.0f : this.Time;
        

        public CinemachineBlendDefinition ToCinemachineBlendDefinition()
        {
            return new CinemachineBlendDefinition()
            {
                m_Style = Style,
                m_Time = BlendTime,
                m_CustomCurve = CustomCurve
            };
        }
    }
}