using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace ScriptHotReload
{
    public static class ExtForLowUnityVersion
    {
#if !UNITY_2021_2_OR_NEWER
        public static bool TryAdd<K,V>(this Dictionary<K, V> dic, K key, V value)
        {
            if (!dic.ContainsKey(key))
            {
                dic.Add(key, value);
                return true;
            }
            else
                return false;
        }
#endif
    }
}

