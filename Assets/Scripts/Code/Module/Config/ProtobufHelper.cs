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

        public static object FromBytes(Type type, byte[] bytes, int index, int count)
        {
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
            stream.Write(ToBytes(message));
        }

        public static object FromStream(Type type, MemoryStream stream)
        {
            object o = Deserializer.Deserialize(type,stream.ToArray());
            if (o is ISupportInitialize supportInitialize)
            {
                supportInitialize.EndInit();
            }
            return o;
        }
    }
}