#if !NOT_UNITY
using UnityEngine;
#endif
using ProtoBuf.Meta;
using System.Numerics;

namespace ProtoBuf
{
    [ProtoContract]
    public class BigIntegerSurrogate
    {
        [ProtoMember(1, OverwriteList = true)]
        public byte[] Data { get; set; }

        public static implicit operator BigInteger(BigIntegerSurrogate surrogate)
            => new BigInteger(surrogate.Data);

        public static implicit operator BigIntegerSurrogate(BigInteger value)
            => new BigIntegerSurrogate { Data = value.ToByteArray() };
    }

    public class UnityTypeBindings
    {
        public static void Init()
        {
            RuntimeTypeModel.Default.Add(typeof(BigInteger), false).SetSurrogate(typeof(BigIntegerSurrogate));
        }
#if !NOT_UNITY
        static UnityTypeBindings()
        {
            var model = RuntimeTypeModel.Default;
            model.Add(typeof(UnityEngine.Vector3), false).SetSurrogate(typeof(Vector3Surrogate));
            model.Add(typeof(UnityEngine.Vector2), false).SetSurrogate(typeof(Vector2Surrogate));
            model.Add(typeof(Color), false).SetSurrogate(typeof(ColorSurrogate));
            model.Add(typeof(LayerMask), false).SetSurrogate(typeof(LayerMaskSurrogate));
        }
       
        [ProtoContract]
        public class Vector3Surrogate
        {
            [ProtoMember(1)] public float X;
            [ProtoMember(2)] public float Y;
            [ProtoMember(3)] public float Z;

            public static implicit operator UnityEngine.Vector3(Vector3Surrogate s)
                => s == null ? UnityEngine.Vector3.zero : new UnityEngine.Vector3(s.X, s.Y, s.Z);

            public static implicit operator Vector3Surrogate(UnityEngine.Vector3 v)
                => new Vector3Surrogate { X = v.x, Y = v.y, Z = v.z };
        }

        [ProtoContract]
        public class Vector2Surrogate
        {
            [ProtoMember(1)] public float X;
            [ProtoMember(2)] public float Y;

            public static implicit operator UnityEngine.Vector2(Vector2Surrogate s)
                => s == null ? UnityEngine.Vector2.zero : new UnityEngine.Vector2(s.X, s.Y);

            public static implicit operator Vector2Surrogate(UnityEngine.Vector2 v)
                => new Vector2Surrogate { X = v.x, Y = v.y };
        }

        [ProtoContract]
        public class ColorSurrogate
        {
            [ProtoMember(1)] public float R;
            [ProtoMember(2)] public float G;
            [ProtoMember(3)] public float B;
            [ProtoMember(4)] public float A;

            public static implicit operator Color(ColorSurrogate s)
                => s == null ? Color.white : new Color(s.R, s.G, s.B, s.A);

            public static implicit operator ColorSurrogate(Color c)
                => new ColorSurrogate { R = c.r, G = c.g, B = c.b, A = c.a };
        }

        [ProtoContract]
        public class LayerMaskSurrogate
        {
            [ProtoMember(1)] public int Value;

            public static implicit operator LayerMask(LayerMaskSurrogate s)
                => s == null ? default : new LayerMask { value = s.Value };

            public static implicit operator LayerMaskSurrogate(LayerMask l)
                => new LayerMaskSurrogate { Value = l.value };
        }
#endif
    }
}