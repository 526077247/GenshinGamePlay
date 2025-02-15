using System.Collections.Generic;
using UnityEngine;
using YooAsset;
namespace TaoTie
{
    public class ConfigLoader : IConfigLoader
    {
        public async ETTask GetAllConfigBytes(Dictionary<string, byte[]> output)
        {
            var assets = YooAssetsMgr.Instance.GetAssetInfos("config",YooAssetsMgr.DefaultName);

            using (ListComponent<ETTask> tasks = new ListComponent<ETTask>())
            {
                foreach (var asset in assets)
                {
                    tasks.Add(LoadConfigBytes(asset,output));
                }

                await ETTaskHelper.WaitAll(tasks);
            }
            
        }
        private async ETTask LoadConfigBytes(AssetInfo asset,Dictionary<string, byte[]> output)
        {
            var op = YooAssetsMgr.Instance.LoadAssetAsync(asset,YooAssetsMgr.DefaultName);
            await op.Task;
            TextAsset v = op.AssetObject as TextAsset;
            string key = asset.Address;
            output[key] = v.bytes;
            op.Release();
        }
        
        public async ETTask<byte[]> GetOneConfigBytes(string configName)
        {
            var op = YooAssets.LoadAssetAsync(configName, TypeInfo<TextAsset>.Type);
            await op.Task;
            TextAsset v = op.AssetObject as TextAsset;
            var bytes = v.bytes;
            op.Release();
            return bytes;
        }
    }
}