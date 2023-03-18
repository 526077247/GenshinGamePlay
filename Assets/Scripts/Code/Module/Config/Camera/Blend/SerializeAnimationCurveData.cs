using System.Collections.Generic;
using LitJson.Extensions;
using Nino.Serialization;
using UnityEngine;

namespace TaoTie
{
    [NinoSerialize]
    public partial class SerializeAnimationCurveData
    {
        [NinoMember(1)]
        public List<SerializeAnimCurveKeyFrame> _animCurveKeyFrameList;

        [JsonIgnore]
        public AnimationCurve AnimCurve
        {
            get
            {
                if (_animCurveKeyFrameList == null || _animCurveKeyFrameList.Count == 0) return null;
                Keyframe[] keyframeArray;
                if (_animCurveKeyFrameList != null)
                {
                    keyframeArray = new Keyframe[_animCurveKeyFrameList.Count];
                    for (int i = 0; i < _animCurveKeyFrameList.Count; i++)
                    {
                        keyframeArray[i] = _animCurveKeyFrameList[i];
                    }
                }
                else
                {
                    keyframeArray = AnimationCurve.Linear(0f, 0f, 1f, 1f).keys;
                }

                return new AnimationCurve(keyframeArray);
            }
            set
            {
                _animCurveKeyFrameList.Clear();
                if (value != null)
                {
                    foreach (var keyframe in value.keys)
                    {
                        _animCurveKeyFrameList.Add(keyframe);
                    }
                }
            }
        }

        public SerializeAnimationCurveData()
        {
            _animCurveKeyFrameList = new List<SerializeAnimCurveKeyFrame>();
        }
    }
}