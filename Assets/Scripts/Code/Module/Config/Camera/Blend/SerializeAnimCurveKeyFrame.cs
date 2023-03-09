using System.Collections.Generic;

using UnityEngine;

namespace TaoTie
{
    public class SerializeAnimCurveKeyFrame 
    {
        private float _time;
        private float _value;
        private float _inTangent;
        private float _outTangent;
        private float _inWeight;
        private float _outWeight;

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