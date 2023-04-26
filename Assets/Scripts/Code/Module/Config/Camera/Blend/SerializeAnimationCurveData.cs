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
        public List<SerializeAnimCurveKeyFrame> AnimCurveKeyFrameList;

        [JsonIgnore]
        public AnimationCurve AnimCurve
        {
            get
            {
                if (AnimCurveKeyFrameList == null || AnimCurveKeyFrameList.Count == 0) return null;
                Keyframe[] keyframeArray;
                if (AnimCurveKeyFrameList != null)
                {
                    keyframeArray = new Keyframe[AnimCurveKeyFrameList.Count];
                    for (int i = 0; i < AnimCurveKeyFrameList.Count; i++)
                    {
                        keyframeArray[i] = AnimCurveKeyFrameList[i];
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
                AnimCurveKeyFrameList.Clear();
                if (value != null)
                {
                    foreach (var keyframe in value.keys)
                    {
                        AnimCurveKeyFrameList.Add(keyframe);
                    }
                }
            }
        }

        public SerializeAnimationCurveData()
        {
            AnimCurveKeyFrameList = new List<SerializeAnimCurveKeyFrame>();
        }
    }
}