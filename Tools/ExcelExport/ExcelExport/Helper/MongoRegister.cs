using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;

namespace TaoTie
{
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
        }
    }
}
