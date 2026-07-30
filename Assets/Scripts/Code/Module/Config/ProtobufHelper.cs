using System;
using ProtoBuf.Meta;
using System.IO;
using ProtoBuf;

namespace TaoTie
{
    public static class ProtobufHelper
    {
        static ProtobufHelper()
        {
            UnityTypeBindings.Init();
        }

        public static T FromBytes<T>(byte[] bytes)
        {
            if (bytes.Length == 0) return default;
            var o = (T) FromBytes(typeof(T), bytes, 0, bytes.Length);
            return o;
        }

        public static object FromBytes(Type type, byte[] bytes, int index, int count)
        {
            using (MemoryStream stream = new MemoryStream(bytes, index, count))
            {
                object o = RuntimeTypeModel.Default.Deserialize(stream, null, type);
                if (o is ISupportInitialize supportInitialize)
                {
                    supportInitialize.EndInit();
                }

                return o;
            }
        }


        public static byte[] ToBytes(object message)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                ProtoBuf.Serializer.Serialize(stream, message);
                return stream.ToArray();
            }
        }

        public static void ToStream(object message, MemoryStream stream)
        {
            ProtoBuf.Serializer.Serialize(stream, message);
        }

        public static object FromStream(Type type, MemoryStream stream)
        {
            object o = RuntimeTypeModel.Default.Deserialize(stream, null, type);
            if (o is ISupportInitialize supportInitialize)
            {
                supportInitialize.EndInit();
            }

            return o;
        }
    }
}