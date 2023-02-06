using System;
using System.Collections.Generic;
#if NOT_UNITY
using System.ComponentModel;
#endif
using System.IO;
using Nino.Serialization;

namespace TaoTie
{
    public static class ProtobufHelper
    {
        public static void Init()
        {
        }
        public static object FromBytes(Type type, byte[] bytes)
        {
            if (bytes.Length == 0) return null;
            object o = Deserializer.Deserialize(type, bytes);
            if (o is ISupportInitialize supportInitialize)
            {
                supportInitialize.EndInit();
            }
            return o;
        }
        public static object FromBytes(Type type, byte[] bytes, int index, int count)
        {
            if (bytes.Length == 0) return null;
            if (index == 0 && count == bytes.Length) return FromBytes(type, bytes);
            var temp = new byte[count - index];
            for (int i = 0; i < temp.Length; i++)
            {
                temp[i] = bytes[index + i];
            }
            object o = Deserializer.Deserialize(type,bytes);
            if (o is ISupportInitialize supportInitialize)
            {
                supportInitialize.EndInit();
            }
            return o;
        }

        public static byte[] ToBytes(object message)
        {
            return Serializer.Serialize(message);
        }

        public static void ToStream(object message, MemoryStream stream)
        {
            var bytes = ToBytes(message);
            stream.Write(bytes,0,bytes.Length);
        }

        public static object FromStream(Type type, MemoryStream stream)
        {
            var bytes = new byte[stream.Length - stream.Position];
            stream.Read(bytes, 0, bytes.Length);
            object o = Deserializer.Deserialize(type,bytes);
            if (o is ISupportInitialize supportInitialize)
            {
                supportInitialize.EndInit();
            }
            return o;
        }
    }
}