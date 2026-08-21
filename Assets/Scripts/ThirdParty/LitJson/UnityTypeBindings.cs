using System;
using System.Numerics;
using TaoTie.LitJson.Extensions;

namespace TaoTie.LitJson
{
    /// <summary>
    /// Unity内建类型拓展
    /// </summary>
    public static class UnityTypeBindings
    {
        static UnityTypeBindings()
        {
            Register();
        }
        public static void Init()
        {
        }
        private static void Register()
        {
            // 注册Type类型的Exporter
            JsonMapper.RegisterExporter<Type>((v, w) =>
            {
                w.Write(v.FullName);
            });

            JsonMapper.RegisterImporter<string, Type>((s) =>
            {
                return Type.GetType(s);
            });
#if !NOT_UNITY
            // 注册Vector2类型的Exporter
            Action<UnityEngine.Vector2, JsonWriter> writeVector2 = (v, w) =>
            {
                w.WriteObjectStart();
                w.WriteProperty("x", v.x);
                w.WriteProperty("y", v.y);
                w.WriteObjectEnd();
            };

            JsonMapper.RegisterExporter<UnityEngine.Vector2>((v, w) =>
            {
                writeVector2(v, w);
            });

            // 注册Vector3类型的Exporter
            Action<UnityEngine.Vector3, JsonWriter> writeVector3 = (v, w) =>
            {
                w.WriteObjectStart();
                w.WriteProperty("x", v.x);
                w.WriteProperty("y", v.y);
                w.WriteProperty("z", v.z);
                w.WriteObjectEnd();
            };

            JsonMapper.RegisterExporter<UnityEngine.Vector3>((v, w) =>
            {
                writeVector3(v, w);
            });

            // 注册Vector4类型的Exporter
            JsonMapper.RegisterExporter<UnityEngine.Vector4>((v, w) =>
            {
                w.WriteObjectStart();
                w.WriteProperty("x", v.x);
                w.WriteProperty("y", v.y);
                w.WriteProperty("z", v.z);
                w.WriteProperty("w", v.w);
                w.WriteObjectEnd();
            });

            // 注册Quaternion类型的Exporter
            JsonMapper.RegisterExporter<UnityEngine.Quaternion>((v, w) =>
            {
                w.WriteObjectStart();
                w.WriteProperty("x", v.x);
                w.WriteProperty("y", v.y);
                w.WriteProperty("z", v.z);
                w.WriteProperty("w", v.w);
                w.WriteObjectEnd();
            });

            // 注册Color类型的Exporter
            JsonMapper.RegisterExporter<UnityEngine.Color>((v, w) =>
            {
                w.WriteObjectStart();
                w.WriteProperty("r", v.r);
                w.WriteProperty("g", v.g);
                w.WriteProperty("b", v.b);
                w.WriteProperty("a", v.a);
                w.WriteObjectEnd();
            });

            // 注册Color32类型的Exporter
            JsonMapper.RegisterExporter<UnityEngine.Color32>((v, w) =>
            {
                w.WriteObjectStart();
                w.WriteProperty("r", v.r);
                w.WriteProperty("g", v.g);
                w.WriteProperty("b", v.b);
                w.WriteProperty("a", v.a);
                w.WriteObjectEnd();
            });

            // 注册Bounds类型的Exporter
            JsonMapper.RegisterExporter<UnityEngine.Bounds>((v, w) =>
            {
                w.WriteObjectStart();

                w.WritePropertyName("center");
                writeVector3(v.center, w);

                w.WritePropertyName("size");
                writeVector3(v.size, w);

                w.WriteObjectEnd();
            });

            // 注册Rect类型的Exporter
            JsonMapper.RegisterExporter<UnityEngine.Rect>((v, w) =>
            {
                w.WriteObjectStart();
                w.WriteProperty("x", v.x);
                w.WriteProperty("y", v.y);
                w.WriteProperty("width", v.width);
                w.WriteProperty("height", v.height);
                w.WriteObjectEnd();
            });

            // 注册RectOffset类型的Exporter
            JsonMapper.RegisterExporter<UnityEngine.RectOffset>((v, w) =>
            {
                w.WriteObjectStart();
                w.WriteProperty("top", v.top);
                w.WriteProperty("left", v.left);
                w.WriteProperty("bottom", v.bottom);
                w.WriteProperty("right", v.right);
                w.WriteObjectEnd();
            });
#endif
            JsonMapper.RegisterExporter<BigInteger>((v, w) =>
            {
                w.Write(v.ToString());
            });
            
            JsonMapper.RegisterImporter<string, BigInteger>((s) =>
            {
                if (string.IsNullOrEmpty(s)) return BigInteger.Zero;
                // 支持科学计数法字符串如 "3.6E+34"
                if (s.Contains('E') || s.Contains('e'))
                {
                    return ParseScientificToBigInteger(s);
                }
                // 寻找小数点
                int dotIndex = s.IndexOf('.');
                if (dotIndex >= 0)
                {
                    s = s.Substring(0, dotIndex);
                }
                return BigInteger.Parse(s);
            });

            // 处理裸数字 3.6E+34：JsonReader 会将其解析为 Double token
            JsonMapper.RegisterImporter<double, BigInteger>((d) =>
            {
                if (d == 0) return BigInteger.Zero;
                return ParseScientificToBigInteger(d.ToString("R"));
            });
            
            JsonMapper.RegisterImporter<string, long>((string input) =>
            {
                if (string.IsNullOrEmpty(input)) return 0L;
                return long.TryParse(input, out long result) ? result : 0L;
            });
            
            JsonMapper.RegisterImporter<string, int>((string input) => {
                if (string.IsNullOrEmpty(input)) return 0;
                return int.TryParse(input, out int result) ? result : 0;
            });
        }

        /// <summary>
        /// 将科学计数法字符串（如 "3.6E+34"）转换为 BigInteger
        /// </summary>
        private static BigInteger ParseScientificToBigInteger(string s)
        {
            int eIndex = s.IndexOfAny(new[] { 'E', 'e' });
            if (eIndex < 0)
            {
                // 非科学计数法，直接处理小数
                int dotIndex = s.IndexOf('.');
                if (dotIndex >= 0)
                    s = s.Substring(0, dotIndex);
                return BigInteger.Parse(s);
            }

            string mantissa = s.Substring(0, eIndex);
            string exponentStr = s.Substring(eIndex + 1);
            int exponent = int.Parse(exponentStr);

            int dotIndex2 = mantissa.IndexOf('.');
            if (dotIndex2 >= 0)
            {
                int fracDigits = mantissa.Length - dotIndex2 - 1;
                mantissa = mantissa.Remove(dotIndex2, 1);
                exponent -= fracDigits;
            }

            BigInteger result = BigInteger.Parse(mantissa);
            if (exponent > 0)
                result *= BigInteger.Pow(10, exponent);
            else if (exponent < 0)
                result /= BigInteger.Pow(10, -exponent);
            return result;
        }

    }
}
