using System.Collections.Generic;
using UnityEngine;

namespace TaoTie
{
    public class CacheManager: IManager
    {
        public static CacheManager Instance;

        private Dictionary<string, object> cacheObj;

        public void Init()
        {
            cacheObj = new Dictionary<string, object>();
            Instance = this;
        }

        public void Destroy()
        {
            Save();
            Instance = null;
        }


        public string GetString(string key, string defaultValue = null)
        {
            return PlayerPrefs.GetString(key, defaultValue);
        }
        
        public int GetInt(string key, int defaultValue = 0)
        {
            return PlayerPrefs.GetInt(key, defaultValue);
        }
        
        public T GetValue<T>(string key) where T: class
        {
            if (cacheObj.TryGetValue(key, out var data))
            {
                return data as T;
            }
            var jStr = PlayerPrefs.GetString(key, null);
            if (jStr == null) return null;
            var res = JsonHelper.FromJson<T>(jStr);
            cacheObj[key] = res;
            return res;
        }
        
        public void SetString(string key, string value)
        {
            PlayerPrefs.SetString(key, value);
        }
        
        public void SetInt(string key, int value)
        {
            PlayerPrefs.SetInt(key, value);
        }
        
        public void SetValue<T>(string key, T value) where T: class
        {
            cacheObj[key] = value;
            var jStr = JsonHelper.ToJson(value);
            PlayerPrefs.SetString(key, jStr);
        }

        public void Save()
        {
            PlayerPrefs.Save();
        }

        public void DeleteKey(string key)
        {
            if (cacheObj.ContainsKey(key))
            {
                cacheObj.Remove(key);
            }
            PlayerPrefs.DeleteKey(key);
        }
    }
}