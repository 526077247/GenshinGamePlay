using System.Collections.Generic;
using Cinemachine;
using LitJson.Extensions;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    public class BlendDefinition
    {
        [Tooltip("Shape of the blend curve")] [SerializeField]
        private CinemachineBlendDefinition.Style _style = CinemachineBlendDefinition.Style.EaseInOut;

        [Tooltip("Duration of the blend, in seconds")]
        [ShowIf("@_style != CinemachineBlendDefinition.Style.Cut")]
        [SerializeField]
        private float _time = 2;

        [ShowIf(nameof(_style), CinemachineBlendDefinition.Style.Custom)] [SerializeField][JsonIgnore]
        private AnimationCurve _customCurve;

        public float blendTime => this._style == CinemachineBlendDefinition.Style.Cut ? 0.0f : this._time;
        public CinemachineBlendDefinition.Style style => _style;
        public AnimationCurve customCurve => _customCurve;

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