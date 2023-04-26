using System.Collections.Generic;
using Nino.Serialization;
using UnityEngine;

namespace TaoTie
{
    [NinoSerialize]
    public partial class SerializeAnimCurveKeyFrame 
    {
        [NinoMember(1)]
        public float Time;
        [NinoMember(2)]
        public float Value;
        [NinoMember(3)]
        public float InTangent;
        [NinoMember(4)]
        public float OutTangent;
        [NinoMember(5)]
        public float InWeight;
        [NinoMember(6)]
        public float OutWeight;
        [NinoMember(7)]
        public WeightedMode Mode;
        public SerializeAnimCurveKeyFrame()
        {
        }

        public SerializeAnimCurveKeyFrame(Keyframe keyframe)
        {
            Time = keyframe.time;
            this.Value = keyframe.value;
            this.InTangent = keyframe.inTangent;
            this.OutTangent = keyframe.outTangent;
            this.InWeight = keyframe.inWeight;
            this.OutWeight = keyframe.outWeight;
            this.Mode = keyframe.weightedMode;
        }

        public static implicit operator SerializeAnimCurveKeyFrame(Keyframe keyframe)
        {
            return new SerializeAnimCurveKeyFrame(keyframe);
        }

        public static implicit operator Keyframe(SerializeAnimCurveKeyFrame keyframe)
        {
            var res=new Keyframe(keyframe.Time, keyframe.Value, keyframe.InTangent, keyframe.OutTangent,
                keyframe.InWeight, keyframe.OutWeight);
            res.weightedMode = keyframe.Mode;
            return res;
        }
    }
}