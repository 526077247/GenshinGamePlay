using System.Collections.Generic;
using UnityEngine;
using YooAsset;
namespace TaoTie
{
    public class ConfigLoader : IConfigLoader
    {
        public void GetAllConfigBytes(Dictionary<string, byte[]> output)
        {
            Dictionary<string, TextAsset> keys = YooAssetsMgr.Instance.LoadAllTextAsset();

            foreach (var kv in keys)
            {
                TextAsset v = kv.Value;
                string key = kv.Key;
                output[key] = v.bytes;
            }
        }

        public byte[] GetOneConfigBytes(string configName)
        {
            TextAsset v = YooAssetsMgr.Instance.LoadTextAsset(configName);
            return v.bytes;
        }
    }
}