using System.Collections.Generic;
using UnityEngine;
using YooAsset;
namespace TaoTie
{
    public class ConfigLoader : IConfigLoader
    {
        public void GetAllConfigBytes(Dictionary<string, byte[]> output)
        {
            var assets = YooAssets.GetAssetInfos("config");

            foreach (var asset in assets)
            {
                var op = YooAssets.LoadAssetSync(asset);
                TextAsset v = op.AssetObject as TextAsset;
                string key = asset.Address;
                output[key] = v.bytes;
                op.Release();
            }
        }

        public byte[] GetOneConfigBytes(string configName)
        {
            var op = YooAssets.LoadAssetAsync(configName,TypeInfo<TextAsset>.Type);
            TextAsset v = op.AssetObject as TextAsset;
            var bytes = v.bytes;
            op.Release();
            return bytes;
        }
    }
}