using System.Collections.Generic;
using Nino.Serialization;
using UnityEngine;

namespace TaoTie
{
    [NinoSerialize]
    public partial class SerializeAnimCurveKeyFrame 
    {
        [NinoMember(1)]
        public float _time;
        [NinoMember(2)]
        public float _value;
        [NinoMember(3)]
        public float _inTangent;
        [NinoMember(4)]
        public float _outTangent;
        [NinoMember(5)]
        public float _inWeight;
        [NinoMember(6)]
        public float _outWeight;

        public SerializeAnimCurveKeyFrame()
        {
        }

        public SerializeAnimCurveKeyFrame(Keyframe keyframe)
        {
            _time = keyframe.time;
            this._value = keyframe.value;
            this._inTangent = keyframe.inTangent;
            this._outTangent = keyframe.outTangent;
            this._inWeight = keyframe.inWeight;
            this._outWeight = keyframe.outWeight;
        }

        public static implicit operator SerializeAnimCurveKeyFrame(Keyframe keyframe)
        {
            return new SerializeAnimCurveKeyFrame(keyframe);
        }

        public static implicit operator Keyframe(SerializeAnimCurveKeyFrame keyframe)
        {
            return new Keyframe(keyframe._time, keyframe._value, keyframe._inTangent, keyframe._outTangent,
                keyframe._inWeight, keyframe._outWeight);
        }
    }
}