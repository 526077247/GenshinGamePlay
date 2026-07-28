using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Serializers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;

namespace TaoTie
{
  
    public class BsonBigIntegerSerializer : SerializerBase<BigInteger>
    {
        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, BigInteger value)
        {
            // 序列化时调用 `ToString()` 将其转换为字符串并写入 BSON
            context.Writer.WriteString(value.ToString());
        }

        public override BigInteger Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            // 反序列化时，从 BSON 中读取字符串，再通过 `BigInteger.Parse` 解析
            var valueStr = context.Reader.ReadString();
            if (BigInteger.TryParse(valueStr, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
                return result;
            throw new Exception("数太大没有转为数值格式");
        }
    }
    public static class MongoRegister
    {
        public static void Init()
        {
        }

        static MongoRegister()
        {
            // 自动注册IgnoreExtraElements

            ConventionPack conventionPack = new ConventionPack { new IgnoreExtraElementsConvention(true) };

            ConventionRegistry.Register("IgnoreExtraElements", conventionPack, type => true);


            var types = typeof(MongoRegister).Assembly.GetTypes();
            foreach (Type type in types)
            {
                if (!type.IsSubclassOf(typeof(Object)))
                {
                    continue;
                }

                if (type.IsGenericType)
                {
                    continue;
                }
                if (type.IsInterface)
                {
                    continue;
                }
                BsonClassMap.LookupClassMap(type);
            }

            // 只需调用一次，推荐放在应用程序启动的最开始
            BsonSerializer.RegisterSerializer(new BsonBigIntegerSerializer());
        }
    }
}
