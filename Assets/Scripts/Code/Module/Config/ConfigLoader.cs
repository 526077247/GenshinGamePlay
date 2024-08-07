using System.Collections.Generic;
using UnityEngine;
using YooAsset;
namespace TaoTie
{
    public class ConfigLoader : IConfigLoader
    {
        public void GetAllConfigBytes(Dictionary<string, byte[]> output)
        {
            var assets = YooAssetsMgr.Instance.GetAssetInfos("config",YooAssetsMgr.DefaultName);

            foreach (var asset in assets)
            {
                var op = YooAssetsMgr.Instance.LoadAssetSync(asset,YooAssetsMgr.DefaultName);
                TextAsset v = op.AssetObject as TextAsset;
                string key = asset.Address;
                output[key] = v.bytes;
                op.Release();
            }
        }

        public byte[] GetOneConfigBytes(string configName)
        {
            var op = YooAssets.LoadAssetSync(configName, TypeInfo<TextAsset>.Type);
            TextAsset v = op.AssetObject as TextAsset;
            var bytes = v.bytes;
            op.Release();
            return bytes;
        }
    }
}