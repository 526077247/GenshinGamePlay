﻿using System;

namespace TaoTie
{
    public static class JsonHelper
    {
       
        public static string ToJson<T>(T message) where T: class
        {
            return LitJson.JsonMapper.ToJson(message);
        }
        
        public static object FromJson(Type type, string json)
        {
            return LitJson.JsonMapper.ToObject(type, json);
        }
        
        public static T FromJson<T>(string json)
        {
            return LitJson.JsonMapper.ToObject<T>(json);
        }
        
        public static bool TryFromJson<T>(string json,out T res)
        {
            try
            {
                res = LitJson.JsonMapper.ToObject<T>(json);
                return res != null;
            }
            catch
            {
                res = default;
                return false;
            }
        }
    }
}